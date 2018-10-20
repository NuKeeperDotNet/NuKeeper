using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Abstract.Configuration;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update.Selection;

namespace NuKeeper.Commands
{
    [HelpOption]
    internal abstract class CommandBase
    {
        private readonly IConfigureLogger _configureLogger;
        protected readonly IFileSettingsCache FileSettingsCache;

        [Option(CommandOptionType.SingleValue, ShortName = "c", LongName = "change",
            Description = "Allowed version change: Patch, Minor, Major. Defaults to Major.")]
        public VersionChange? AllowedChange { get; set; }

        [Option(CommandOptionType.MultipleValue, ShortName = "s", LongName = "source",
            Description =
                "Specifies a NuGet package source to use during the operation. This setting overrides all of the sources specified in the NuGet.config files. Multiple sources can be provided by specifying this option multiple times.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string[] Source { get; }

        protected NuGetSources NuGetSources => Source == null?  null : new NuGetSources(Source);

        [Option(CommandOptionType.SingleValue, ShortName = "v", LongName = "verbosity",
            Description = "Sets the verbosity level of the command. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed].")]
        public LogLevel? Verbosity { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "lf", LongName = "logfile",
            Description = "Log to the named file")]
        public string LogFile { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "a", LongName = "age",
            Description = "Exclude updates that do not meet a minimum age, in order to not consume packages immediately after they are released. Examples: 0 = zero, 12h = 12 hours, 3d = 3 days, 2w = two weeks. The default is 7 days.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string MinimumPackageAge { get; }

        [Option(CommandOptionType.SingleValue, ShortName = "i", LongName = "include",
            Description = "Only consider packages matching this regex pattern.")]
        public string Include { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "e", LongName = "exclude",
            Description = "Do not consider packages matching this regex pattern.")]
        public string Exclude { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "om", LongName = "outputformat",
            Description = "Format for output.")]
        public OutputFormat? OutputFormat { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "od", LongName = "outputdestination",
            Description = "Destination for output.")]
        public OutputDestination? OutputDestination { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "of", LongName = "outputfile",
            Description = "File name for output.")]
        public string OutputFileName { get; set; }

        protected CommandBase(IConfigureLogger logger, IFileSettingsCache fileSettingsCache)
        {
            _configureLogger = logger;
            FileSettingsCache = fileSettingsCache;
        }

        public async Task<int> OnExecute()
        {
            InitialiseLogging();

            var settings = MakeSettings();

            var validationResult = PopulateSettings(settings);
            if (!validationResult.IsSuccess)
            {
                var logger = _configureLogger as INuKeeperLogger;
                logger?.Error(validationResult.ErrorMessage);
                return -1;
            }

            return await Run(settings);
        }

        private void InitialiseLogging()
        {
            var fileSettings = FileSettingsCache.GetSettings();
            var logLevel = Concat.FirstValue(Verbosity, fileSettings.Verbosity, LogLevel.Normal);
            var logFile = Concat.FirstValue(LogFile, fileSettings.LogFile);

            _configureLogger.Initialise(logLevel, logFile);
        }

        private SettingsContainer MakeSettings()
        {
            var fileSettings = FileSettingsCache.GetSettings();
            var allowedChange = Concat.FirstValue(AllowedChange, fileSettings.Change, VersionChange.Major);

            var settings = new SettingsContainer
            {
                SourceControlServerSettings = new SourceControlServerSettings(),
                PackageFilters = new FilterSettings(),
                UserSettings = new UserSettings
                {
                    AllowedChange = allowedChange,
                    NuGetSources = NuGetSources
                }
            };

            return settings;
        }

        protected virtual ValidationResult PopulateSettings(SettingsContainer settings)
        {
            var minPackageAge = ReadMinPackageAge();
            if (!minPackageAge.HasValue)
            {
                return ValidationResult.Failure($"Min package age '{MinimumPackageAge}' could not be parsed");
            }

            settings.PackageFilters.MinimumAge = minPackageAge.Value;

            var regexIncludeValid = PopulatePackageIncludes(settings);
            if (!regexIncludeValid.IsSuccess)
            {
                return regexIncludeValid;
            }

            var regexExcludeValid = PopulatePackageExcludes(settings);
            if (!regexExcludeValid.IsSuccess)
            {
                return regexExcludeValid;
            }

            var settingsFromFile = FileSettingsCache.GetSettings();

            var defaultOutputDestination = string.IsNullOrWhiteSpace(OutputFileName)
                ? Inspection.Report.OutputDestination.Console
                : Inspection.Report.OutputDestination.File;

            settings.UserSettings.OutputDestination =
                Concat.FirstValue(OutputDestination, settingsFromFile.OutputDestination,
                    defaultOutputDestination);

            settings.UserSettings.OutputFormat =
                Concat.FirstValue(OutputFormat, settingsFromFile.OutputFormat,
                    Inspection.Report.OutputFormat.Text);

            settings.UserSettings.OutputFileName =
                Concat.FirstValue(OutputFileName, settingsFromFile.OutputFileName,
                    "nukeeper.out");

            return ValidationResult.Success;
        }

        private TimeSpan? ReadMinPackageAge()
        {
            const string defaultMinPackageAge = "7d";
            var settingsFromFile = FileSettingsCache.GetSettings();
            var valueWithFallback = Concat.FirstValue(MinimumPackageAge, settingsFromFile.Age, defaultMinPackageAge);

            return DurationParser.Parse(valueWithFallback);
        }

        private ValidationResult PopulatePackageIncludes(
            SettingsContainer settings)
        {
            var settingsFromFile = FileSettingsCache.GetSettings();
            var value = Concat.FirstValue(Include, settingsFromFile.Include);

            if (string.IsNullOrWhiteSpace(value))
            {
                settings.PackageFilters.Includes = null;
                return ValidationResult.Success;
            }

            try
            {
                settings.PackageFilters.Includes = new Regex(value);
            }
            catch (Exception ex)
            {
                {
                    return ValidationResult.Failure(
                        $"Unable to parse regex '{value}' for Include: {ex.Message}");
                }
            }

            return ValidationResult.Success;
        }

        private ValidationResult PopulatePackageExcludes(
            SettingsContainer settings)
        {
            var settingsFromFile = FileSettingsCache.GetSettings();
            var value = Concat.FirstValue(Exclude, settingsFromFile.Exclude);

            if (string.IsNullOrWhiteSpace(value))
            {
                settings.PackageFilters.Excludes = null;
                return ValidationResult.Success;
            }

            try
            {
                settings.PackageFilters.Excludes = new Regex(value);
            }
            catch (Exception ex)
            {
                {
                    return ValidationResult.Failure(
                        $"Unable to parse regex '{value}' for Exclude: {ex.Message}");
                }
            }

            return ValidationResult.Success;
        }

        protected abstract Task<int> Run(SettingsContainer settings);
    }
}
