using System;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Logging;

namespace NuKeeper
{
    [Command(Description = "Performs version checks and generates updates for a single repository.")]
    internal class RepositoryCommand : GitHubNuKeeperCommand
    {
        private readonly GithubEngine _engine;

        [Argument(0, Name = "GitHub repository uri", Description = "The repository to scan.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        protected Uri GitHubRepositoryUri { get; }

        public RepositoryCommand(GithubEngine engine, IConfigureLogLevel logger) : base(logger)
        {
            _engine = engine;
        }

        protected override async Task<int> Run(SettingsContainer settings)
        {
            await base.Run(settings);
            settings.ModalSettings.Mode = RunMode.Repository;
            settings.ModalSettings.Repository = ReadRepositorySettings(GitHubRepositoryUri);

            await _engine.Run(settings);
            return 0;
        }

        private static RepositorySettings ReadRepositorySettings(Uri gitHubRepositoryUri)
        {
            // general pattern is https://github.com/owner/reponame.git
            // from this we extract owner and repo name
            var path = gitHubRepositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var repoOwner = pathParts[0];
            var repoName = pathParts[1].Replace(".git", string.Empty);

            return new RepositorySettings
            {
                GithubUri = gitHubRepositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = repoOwner
            };
        }
    }
}
