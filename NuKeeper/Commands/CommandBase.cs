using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Commands
{
    [HelpOption]
    internal abstract class CommandBase
    {
        private readonly IConfigureLogLevel _logger;

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
                "In order to not consume packages immediately after they are released, exclude updates that do not meet a minimum age. Examples: 0 = zero, 12h = 12 hours, 3d = 3 days, 2w = two weeks. The default is 7 days.")]
        // ReSharper disable once MemberCanBePrivate.Global
        protected string MinimumPackageAge { get; } = "7d";

        [Option(CommandOptionType.SingleValue, ShortName = "i", LongName = "include", Description = "Only consider packages matching this regex pattern.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string Include { get; }

        [Option(CommandOptionType.SingleValue, ShortName = "e", LongName = "exclude", Description = "Do not consider packages matching this regex pattern.")]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        protected string Exclude { get; }

        protected CommandBase(IConfigureLogLevel logger)
        {
            _logger = logger;
        }


        // ReSharper disable once UnusedMember.Global
        public async Task<int> OnExecute()
        {
            _logger.SetLogLevel(Verbosity);
            var settings = MakeSettings();

            var validationResult = ValidateSettings(settings);
            if (!validationResult.IsSuccess)
            {
                Console.WriteLine(validationResult.ErrorMessage);
                return -1;
            }

            return await Run(settings);
        }

        private SettingsContainer MakeSettings()
        {
            var minPackageAge = DurationParser.Parse(MinimumPackageAge);
            if (!minPackageAge.HasValue)
            {
                minPackageAge = TimeSpan.Zero;
                Console.WriteLine($"Min package age '{MinimumPackageAge}' could not be parsed");
            }

            var settings = new SettingsContainer
            {
                ModalSettings = new ModalSettings(),
                UserSettings = new UserSettings
                {
                    AllowedChange = AllowedChange,
                    NuGetSources = NuGetSources,
                    MinimumPackageAge = minPackageAge.Value,
                    PackageIncludes = SettingsParser.ParseRegex(Include, nameof(Include)),
                    PackageExcludes = SettingsParser.ParseRegex(Exclude, nameof(Exclude)),
                }
            };

            PopulateSettings(settings);
            return settings;
        }

        protected virtual void PopulateSettings(SettingsContainer settings)
        {
        }

        protected virtual ValidationResult ValidateSettings(SettingsContainer settings)
        {
            return ValidationResult.Success;
        }

        protected abstract Task<int> Run(SettingsContainer settings);
    }
}
