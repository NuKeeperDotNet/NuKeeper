using System;
using System.Threading.Tasks;
using LibGit2Sharp;
using NuKeeper.Configuration;
using NuKeeper.Creators;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Engine
{
    public class GitHubEngine
    {
        private readonly ICreate<IGitHub> _githubCreator;
        private readonly ICreate<IGitHubRepositoryDiscovery> _repositoryDiscoveryCreator;
        private readonly ICreate<IGitHubRepositoryEngine> _repositoryEngineCreator;
        private readonly IFolderFactory _folderFactory;
        private readonly INuKeeperLogger _logger;

        public GitHubEngine(
            ICreate<IGitHub> githubCreator,
            ICreate<IGitHubRepositoryDiscovery> repositoryDiscoveryCreator,
            ICreate<IGitHubRepositoryEngine> repositoryEngineCreator,
            IFolderFactory folderFactory,
            INuKeeperLogger logger)
        {
            _githubCreator = githubCreator;
            _repositoryDiscoveryCreator = repositoryDiscoveryCreator;
            _repositoryEngineCreator = repositoryEngineCreator;
            _folderFactory = folderFactory;
            _logger = logger;
        }

        public async Task<int> Run(SettingsContainer settings)
        {
            var github = _githubCreator.Create(settings);
            var repositoryDiscovery = _repositoryDiscoveryCreator.Create(settings);
            var repositoryEngine = _repositoryEngineCreator.Create(settings);

            _logger.Verbose($"{Now()}: Started");

            _folderFactory.DeleteExistingTempDirs();

            var githubUser = await github.GetCurrentUser();
            var gitCreds = new UsernamePasswordCredentials
            {
                Username = githubUser.Login,
                Password = settings.GithubAuthSettings.Token
            };

            var userIdentity = GetUserIdentity(githubUser);

            var repositories = await repositoryDiscovery.GetRepositories();

            var reposUpdated = 0;

            foreach (var repository in repositories)
            {
                if (reposUpdated >= settings.UserSettings.MaxRepositoriesChanged)
                {
                    _logger.Verbose($"Reached max of {reposUpdated} repositories changed");
                    break;
                }

                var updatesInThisRepo = await repositoryEngine.Run(repository, gitCreds, userIdentity);
                if (updatesInThisRepo > 0)
                {
                    reposUpdated++;
                }
            }

            if (reposUpdated > 1)
            {
                _logger.Verbose($"{reposUpdated} repositories were updated");
            }

            _logger.Verbose($"Done at {Now()}");
            return reposUpdated;
        }

        private Identity GetUserIdentity(IGitHubAccount githubUser)
        {
            if (string.IsNullOrWhiteSpace(githubUser?.Name))
            {
                _logger.Terse("GitHub user name missing from profile, falling back to .gitconfig");
                return null;
            }
            if (string.IsNullOrWhiteSpace(githubUser?.Email))
            {
                _logger.Terse("GitHub public email missing from profile, falling back to .gitconfig");
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
