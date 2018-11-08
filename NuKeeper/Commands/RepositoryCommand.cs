using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Logging;
using System;
using System.Linq;
using NuKeeper.Abstractions;

namespace NuKeeper.Commands
{
    [Command(Description = "Performs version checks and generates pull requests for a single repository.")]
    internal class RepositoryCommand : GitHubNuKeeperCommand
    {
        [Argument(0, Name = "Repository URI", Description = "The URI of the repository to scan.")]
        public string GitHubRepositoryUri { get; set; }

        public RepositoryCommand(ICollaborationEngine engine, IConfigureLogger logger, IFileSettingsCache fileSettingsCache, ICollaborationFactory collaborationFactory)
            : base(engine, logger, fileSettingsCache, collaborationFactory)
        {
        }

        protected override ValidationResult PopulateSettings(SettingsContainer settings)
        {
            if (!Uri.TryCreate(GitHubRepositoryUri, UriKind.Absolute, out var repoUri))
            {
                return ValidationResult.Failure($"Bad repository URI: '{GitHubRepositoryUri}'");
            }
            var fileSettings = FileSettingsCache.GetSettings();
            var endpoint = Concat.FirstValue(GithubApiEndpoint, fileSettings.Api);

            if (repoUri.Host == "github.com")
            {
                GithubApiEndpoint = endpoint ?? "https://api.github.com/";
            }

            if (repoUri.Host == "dev.azure.com")
            {
                var path = repoUri.AbsolutePath;
                var pathParts = path.Split('/')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                GithubApiEndpoint = endpoint ?? $"https://dev.azure.com/{pathParts[0]}";
            }

            var baseResult = base.PopulateSettings(settings);
            if (!baseResult.IsSuccess)
            {
                return baseResult;
            }

            settings.SourceControlServerSettings.Repository = CollaborationFactory.SettingsReader.RepositorySettings(repoUri);
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
