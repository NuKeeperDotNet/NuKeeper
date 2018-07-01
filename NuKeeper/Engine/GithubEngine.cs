using System;
using System.Threading.Tasks;
using LibGit2Sharp;
using NuKeeper.Configuration;
using NuKeeper.Github;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.Logging;
using Octokit;

namespace NuKeeper.Engine
{
    public class GithubEngine
    {
        private readonly IGithub _github;
        private readonly IGithubRepositoryDiscovery _repositoryDiscovery;
        private readonly IGithubRepositoryEngine _repositoryEngine;
        private readonly UserSettings _userSettings;
        private readonly string _githubToken;
        private readonly IFolderFactory _folderFactory;
        private readonly INuKeeperLogger _logger;

        public GithubEngine(
            IGithub github,
            IGithubRepositoryDiscovery repositoryDiscovery,
            IGithubRepositoryEngine repositoryEngine,
            UserSettings userSettings,
            GithubAuthSettings settings,
            IFolderFactory folderFactory,
            INuKeeperLogger logger)
        {
            _github = github;
            _repositoryDiscovery = repositoryDiscovery;
            _repositoryEngine = repositoryEngine;
            _userSettings = userSettings;
            _githubToken = settings.Token;
            _folderFactory = folderFactory;
            _logger = logger;
        }

        public async Task<int> Run()
        {
            _logger.Verbose($"{Now()}: Started");

            _folderFactory.DeleteExistingTempDirs();

            var githubUser = await _github.GetCurrentUser();
            var gitCreds = new UsernamePasswordCredentials
            {
                Username = githubUser.Login,
                Password = _githubToken
            };

            var userIdentity = GetUserIdentity(githubUser);

            var repositories = await _repositoryDiscovery.GetRepositories();

            var reposUpdated = 0;

            foreach (var repository in repositories)
            {
                if (reposUpdated >= _userSettings.MaxRepositoriesChanged)
                {
                    _logger.Verbose($"Reached max of {reposUpdated} repositories changed");
                    break;
                }

                var updatesInThisRepo = await _repositoryEngine.Run(repository, gitCreds, userIdentity);
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

        private Identity GetUserIdentity(Account githubUser)
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
