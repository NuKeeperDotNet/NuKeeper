using System;
using System.Text.RegularExpressions;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Commands
{
    internal abstract class MultipleRepositoryCommand : GitHubNuKeeperCommand
    {
        [Option(CommandOptionType.SingleValue, ShortName = "ir", LongName = "includerepos", Description = "Only consider repositories matching this regex pattern.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string IncludeRepos { get; }

        [Option(CommandOptionType.SingleValue, ShortName = "er", LongName = "excluderepos", Description = "Do not consider repositories matching this regex pattern.")]
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        protected string ExcludeRepos { get; }

        protected MultipleRepositoryCommand(IGitHubEngine engine, IConfigureLogLevel logger, IFileSettingsCache fileSettingsCache)
            : base(engine, logger, fileSettingsCache)
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

            return ValidationResult.Success;
        }

        private ValidationResult PopulateIncludeRepos(SettingsContainer settings)
        {
            var settingsFromFile = FileSettingsCache.Get();
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
            var settingsFromFile = FileSettingsCache.Get();
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
