using System;
using System.Text.RegularExpressions;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Collaboration;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Commands
{
    internal abstract class MultipleRepositoryCommand : CollaborationPlatformCommand
    {
        [Option(CommandOptionType.SingleValue, ShortName = "ir", LongName = "includerepos", Description = "Only consider repositories matching this regex pattern.")]
        public string IncludeRepos { get; set;  }

        [Option(CommandOptionType.SingleValue, ShortName = "er", LongName = "excluderepos", Description = "Do not consider repositories matching this regex pattern.")]
        public string ExcludeRepos { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "x", LongName = "maxrepo",
            Description = "The maximum number of repositories to change. Defaults to 10.")]
        public int? AllowedMaxRepositoriesChangedChange { get; set; }


        protected MultipleRepositoryCommand(ICollaborationEngine engine, IConfigureLogger logger, IFileSettingsCache fileSettingsCache, ICollaborationFactory collaborationFactory)
            : base(engine, logger, fileSettingsCache, collaborationFactory)
        {
        }

        protected override ValidationResult PopulateSettings(SettingsContainer settings)
        {
            var baseResult = base.PopulateSettings(settings);
            if (!baseResult.IsSuccess)
            {
                return baseResult;
            }

            var regexIncludeReposValid = PopulateIncludeRepos(settings);
            if (!regexIncludeReposValid.IsSuccess)
            {
                return regexIncludeReposValid;
            }

            var regexExcludeReposValid = PopulateExcludeRepos(settings);
            if (!regexExcludeReposValid.IsSuccess)
            {
                return regexExcludeReposValid;
            }

            var fileSettings = FileSettingsCache.GetSettings();

            const int defaultMaxReposChanged = 10;
            settings.UserSettings.MaxRepositoriesChanged = Concat.FirstValue(
                AllowedMaxRepositoriesChangedChange, fileSettings.MaxRepo, defaultMaxReposChanged);

            return ValidationResult.Success;
        }

        private ValidationResult PopulateIncludeRepos(SettingsContainer settings)
        {
            var settingsFromFile = FileSettingsCache.GetSettings();
            var value = Concat.FirstValue(IncludeRepos, settingsFromFile.IncludeRepos);

            if (string.IsNullOrWhiteSpace(value))
            {
                settings.SourceControlServerSettings.IncludeRepos = null;
                return ValidationResult.Success;
            }

            try
            {
                settings.SourceControlServerSettings.IncludeRepos = new Regex(value);
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure($"Unable to parse regex '{value}' for IncludeRepos: {ex.Message}");
            }

            return ValidationResult.Success;
        }

        private ValidationResult PopulateExcludeRepos(SettingsContainer settings)
        {
            var settingsFromFile = FileSettingsCache.GetSettings();
            var value = Concat.FirstValue(ExcludeRepos, settingsFromFile.ExcludeRepos);

            if (string.IsNullOrWhiteSpace(value))
            {
                settings.SourceControlServerSettings.ExcludeRepos = null;
                return ValidationResult.Success;
            }

            try
            {
                settings.SourceControlServerSettings.ExcludeRepos = new Regex(value);
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure($"Unable to parse regex '{value}' for ExcludeRepos: {ex.Message}");
            }

            return ValidationResult.Success;
        }
    }
}
