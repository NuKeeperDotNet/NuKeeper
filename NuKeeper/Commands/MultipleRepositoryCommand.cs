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

        protected MultipleRepositoryCommand(GitHubEngine engine, IConfigureLogLevel logger, IFileSettingsCache fileSettingsCache)
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

            var regexIncludeReposValid = PopulateIncludeRepos(settings, IncludeRepos);
            if (!regexIncludeReposValid.IsSuccess)
            {
                return regexIncludeReposValid;
            }

            var regexExcludeReposValid = PopulateExcludeRepos(settings, ExcludeRepos);
            if (!regexExcludeReposValid.IsSuccess)
            {
                return regexExcludeReposValid;
            }

            return ValidationResult.Success;
        }

        private static ValidationResult PopulateIncludeRepos(SettingsContainer settings, string value)
        {
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

        private static ValidationResult PopulateExcludeRepos(SettingsContainer settings, string value)
        {
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
