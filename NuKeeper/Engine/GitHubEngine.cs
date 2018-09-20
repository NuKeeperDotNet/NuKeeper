using System;
using System.Threading.Tasks;
using LibGit2Sharp;
using NuKeeper.Configuration;
using NuKeeper.Creators;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.Logging;
using Octokit;

namespace NuKeeper.Engine
{
    public class GitHubEngine : IGitHubEngine
    {
        private readonly IGitHub _github;
        private readonly IGitHubRepositoryDiscovery _repositoryDiscovery;
        private readonly ICreate<IGitHubRepositoryEngine> _repositoryEngineCreator;
        private readonly IFolderFactory _folderFactory;
        private readonly INuKeeperLogger _logger;

        public GitHubEngine(
            IGitHub github,
            IGitHubRepositoryDiscovery repositoryDiscovery,
            ICreate<IGitHubRepositoryEngine> repositoryEngineCreator,
            IFolderFactory folderFactory,
            INuKeeperLogger logger)
        {
            _github = github;
            _repositoryDiscovery = repositoryDiscovery;
            _repositoryEngineCreator = repositoryEngineCreator;
            _folderFactory = folderFactory;
            _logger = logger;
        }

        public async Task<int> Run(SettingsContainer settings)
        {
            _github.Initialise(settings.GithubAuthSettings);
            var repositoryEngine = _repositoryEngineCreator.Create(settings);

            _logger.Detailed($"{Now()}: Started");

            _folderFactory.DeleteExistingTempDirs();

            var githubUser = await _github.GetCurrentUser();
            var gitCreds = new UsernamePasswordCredentials
            {
                Username = githubUser.Login,
                Password = settings.GithubAuthSettings.Token
            };

            var userIdentity = GetUserIdentity(githubUser);

            var repositories = await _repositoryDiscovery.GetRepositories(_github, settings.SourceControlServerSettings);

            var reposUpdated = 0;

            foreach (var repository in repositories)
            {
                if (reposUpdated >= settings.UserSettings.MaxRepositoriesChanged)
                {
                    _logger.Detailed($"Reached max of {reposUpdated} repositories changed");
                    break;
                }

                var updatesInThisRepo = await repositoryEngine.Run(repository,
                    gitCreds, userIdentity, settings);

                if (updatesInThisRepo > 0)
                {
                    reposUpdated++;
                }
            }

            if (reposUpdated > 1)
            {
                _logger.Detailed($"{reposUpdated} repositories were updated");
            }

            _logger.Detailed($"Done at {Now()}");
            return reposUpdated;
        }

        private Identity GetUserIdentity(Account githubUser)
        {
            if (string.IsNullOrWhiteSpace(githubUser?.Name))
            {
                _logger.Minimal("GitHub user name missing from profile, falling back to .gitconfig");
                return null;
            }
            if (string.IsNullOrWhiteSpace(githubUser?.Email))
            {
                _logger.Minimal("GitHub public email missing from profile, falling back to .gitconfig");
                return null;
            }

            return new Identity(githubUser.Name, githubUser.Email);
        }

        private static string Now()
        {
            return DateFormat.AsUtcIso8601(DateTimeOffset.Now);
        }
    }
}
