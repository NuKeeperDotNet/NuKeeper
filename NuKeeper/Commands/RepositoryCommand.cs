using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Logging;
using System;
using System.Collections.Generic;
using NuKeeper.Abstractions;

namespace NuKeeper.Commands
{
    [Command(Description = "Performs version checks and generates pull requests for a single repository.")]
    internal class RepositoryCommand : GitHubNuKeeperCommand
    {
        [Argument(0, Name = "Repository URI", Description = "The URI of the repository to scan.")]
        public string GitHubRepositoryUri { get; set; }

        private readonly IEnumerable<ISettingsReader> _settingsReaders;

        public RepositoryCommand(ICollaborationEngine engine, IConfigureLogger logger, IFileSettingsCache fileSettingsCache, ICollaborationFactory collaborationFactory, IEnumerable<ISettingsReader> settingsReaders)
            : base(engine, logger, fileSettingsCache, collaborationFactory)
        {
            _settingsReaders = settingsReaders;
        }

        protected override ValidationResult PopulateSettings(SettingsContainer settings)
        {
            if (!Uri.TryCreate(GitHubRepositoryUri, UriKind.Absolute, out var repoUri))
            {
                return ValidationResult.Failure($"Bad repository URI: '{GitHubRepositoryUri}'");
            }
            var fileSettings = FileSettingsCache.GetSettings();
            var endpoint = Concat.FirstValue(GithubApiEndpoint, fileSettings.Api);

            RepositorySettings repositorySettings = null;
            foreach (var reader in _settingsReaders)
            {
                if (reader.CanRead(repoUri))
                {
                    repositorySettings = reader.RepositorySettings(repoUri);
                    GithubApiEndpoint = endpoint ?? repositorySettings.ApiUri.ToString();
                }
            }

            var baseResult = base.PopulateSettings(settings);
            if (!baseResult.IsSuccess)
            {
                return baseResult;
            }

            settings.SourceControlServerSettings.Repository = repositorySettings;
            if (settings.SourceControlServerSettings.Repository == null)
            {
                return ValidationResult.Failure($"Could not read repository URI: '{GitHubRepositoryUri}'");
            }

            settings.SourceControlServerSettings.Scope = ServerScope.Repository;
            settings.UserSettings.MaxRepositoriesChanged = 1;
            return ValidationResult.Success;
        }
    }
}
