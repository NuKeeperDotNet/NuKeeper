using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update.Selection;

namespace NuKeeper.Commands
{
    [HelpOption]
    internal abstract class CommandBase
    {
        private readonly IConfigureLogLevel _configureLogger;
        protected readonly IFileSettingsCache FileSettingsCache;

        [Option(CommandOptionType.SingleValue, ShortName = "c", LongName = "change",
            Description = "Allowed version change: Patch, Minor, Major. Defaults to Major.")]
        protected VersionChange AllowedChange { get; } = VersionChange.Major;

        [Option(CommandOptionType.MultipleValue, ShortName = "s", LongName = "source",
            Description =
                "Specifies a NuGet package source to use during the operation. This setting overrides all of the sources specified in the NuGet.config files. Multiple sources can be provided by specifying this option multiple times.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string[] Source { get; }

        protected NuGetSources NuGetSources => Source == null?  null : new NuGetSources(Source);

        [Option(CommandOptionType.SingleValue, ShortName = "v", LongName = "verbosity", Description = "Sets the verbosity level of the command. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed].")]
        // ReSharper disable once MemberCanBePrivate.Global
        protected LogLevel Verbosity { get; } = LogLevel.Normal;

        [Option(CommandOptionType.SingleValue, ShortName = "a", LongName = "age",
            Description =
                "Exclude updates that do not meet a minimum age, in order to not consume packages immediately after they are released. Examples: 0 = zero, 12h = 12 hours, 3d = 3 days, 2w = two weeks. The default is 7 days.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string MinimumPackageAge { get; } 

        [Option(CommandOptionType.SingleValue, ShortName = "i", LongName = "include", Description = "Only consider packages matching this regex pattern.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string Include { get; }

        [Option(CommandOptionType.SingleValue, ShortName = "e", LongName = "exclude", Description = "Do not consider packages matching this regex pattern.")]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        protected string Exclude { get; }

        protected CommandBase(IConfigureLogLevel logger, IFileSettingsCache fileSettingsCache)
        {
            _configureLogger = logger;
            FileSettingsCache = fileSettingsCache;
        }


        // ReSharper disable once UnusedMember.Global
        public async Task<int> OnExecute()
        {
            _configureLogger.SetLogLevel(Verbosity);
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

        private SettingsContainer MakeSettings()
        {
            var settings = new SettingsContainer
            {
                SourceControlServerSettings = new SourceControlServerSettings(),
                PackageFilters = new FilterSettings(),
                UserSettings = new UserSettings
                {
                    AllowedChange = AllowedChange,
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

            return ValidationResult.Success;
        }

        private TimeSpan? ReadMinPackageAge()
        {
            const string defaultMinPackageAge = "7d";
            var settingsFromFile = FileSettingsCache.Get();
            var valueWithFallback = Concat.FirstValue(MinimumPackageAge, settingsFromFile.Age, defaultMinPackageAge);

            return DurationParser.Parse(valueWithFallback);
        }

        private ValidationResult PopulatePackageIncludes(
            SettingsContainer settings)
        {
            var settingsFromFile = FileSettingsCache.Get();
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
            var settingsFromFile = FileSettingsCache.Get();
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
