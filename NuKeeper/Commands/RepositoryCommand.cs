using System;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Commands
{
    [Command(Description = "Performs version checks and generates pull requests for a single repository.")]
    internal class RepositoryCommand : GitHubNuKeeperCommand
    {

        [Argument(0, Name = "Repository URI", Description = "The URI of the repository to scan.")]
        public string GitHubRepositoryUri { get; set; }

        public RepositoryCommand(ICollaborationEngine engine, IConfigureLogger logger, IFileSettingsCache fileSettingsCache, ISettingsReader settingsReader)
            : base(engine, logger, fileSettingsCache, settingsReader)
        {
        }

        protected override ValidationResult PopulateSettings(SettingsContainer settings)
        {
            var baseResult = base.PopulateSettings(settings);
            if (!baseResult.IsSuccess)
            {
                return baseResult;
            }

            if (!Uri.TryCreate(GitHubRepositoryUri, UriKind.Absolute, out var repoUri))
            {
                return ValidationResult.Failure($"Bad repository URI: '{GitHubRepositoryUri}'");
            }

            settings.SourceControlServerSettings.Repository = SettingsReader.RepositorySettings(repoUri);
            if (settings.SourceControlServerSettings.Repository == null)
            {
                return ValidationResult.Failure($"Could not read repository URI: '{GitHubRepositoryUri}'");
            }

            settings.UserSettings.MaxRepositoriesChanged = 1;
            settings.SourceControlServerSettings.Scope = ServerScope.Repository;
            return ValidationResult.Success;
        }
    }
}
