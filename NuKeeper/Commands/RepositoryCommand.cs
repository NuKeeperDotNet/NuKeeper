using System;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Commands
{
    [Command(Description = "Performs version checks and generates pull requests for a single repository on GitHub.")]
    internal class RepositoryCommand : GitHubNuKeeperCommand
    {
        private readonly ISettingsReader _settingsReader;

        [Argument(0, Name = "GitHub repository URI", Description = "The URI of the repository to scan.")]
        public string GitHubRepositoryUri { get; set; }

        public RepositoryCommand(IEngine engine, IConfigureLogger logger, IFileSettingsCache fileSettingsCache, ISettingsReader settingsReader)
            : base(engine, logger, fileSettingsCache)
        {
            _settingsReader = settingsReader;
        }

        protected override ValidationResult PopulateSettings(SettingsContainer settings)
        {
            var baseResult = base.PopulateSettings(settings);
            if (!baseResult.IsSuccess)
            {
                return baseResult;
            }

            if (!Uri.TryCreate(GitHubRepositoryUri, UriKind.Absolute, out Uri repoUri))
            {
                return ValidationResult.Failure($"Bad GitHub repository URI: '{GitHubRepositoryUri}'");
            }

            settings.SourceControlServerSettings.Repository = _settingsReader.ReadRepositorySettings(repoUri);

            if (settings.SourceControlServerSettings.Repository == null)
            {
                return ValidationResult.Failure($"Could not read GitHub repository URI: '{GitHubRepositoryUri}'");
            }

            settings.UserSettings.MaxRepositoriesChanged = 1;
            settings.SourceControlServerSettings.Scope = ServerScope.Repository;
            return ValidationResult.Success;
        }
    }
}
