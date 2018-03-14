using System;
using System.Threading.Tasks;
using LibGit2Sharp;
using NuKeeper.Configuration;
using NuKeeper.Github;
using NuKeeper.Files;
using NuKeeper.Logging;
using Octokit;

namespace NuKeeper.Engine
{
    public class GithubEngine
    {
        private readonly IGithub _github;
        private readonly IGithubRepositoryDiscovery _repositoryDiscovery;
        private readonly IGithubRepositoryEngine _repositoryEngine;
        private readonly string _githubToken;
        private readonly IFolderFactory _folderFactory;
        private readonly INuKeeperLogger _logger;

        public GithubEngine(
            IGithub github,
            IGithubRepositoryDiscovery repositoryDiscovery,
            IGithubRepositoryEngine repositoryEngine,
            GithubAuthSettings settings,
            IFolderFactory folderFactory,
            INuKeeperLogger logger)
        {
            _github = github;
            _repositoryDiscovery = repositoryDiscovery;
            _repositoryEngine = repositoryEngine;
            _githubToken = settings.Token;
            _folderFactory = folderFactory;
            _logger = logger;
        }

        public async Task Run()
        {
            _folderFactory.DeleteExistingTempDirs();

            var githubUser = await _github.GetCurrentUser();
            var gitCreds = new UsernamePasswordCredentials
            {
                Username = githubUser.Login,
                Password = _githubToken
            };

            var userIdentity = GetUserIdentity(githubUser);

            var repositories = await _repositoryDiscovery.GetRepositories();

            foreach (var repository in repositories)
            {
                await _repositoryEngine.Run(repository, gitCreds, userIdentity);
            }
        }

        private Identity GetUserIdentity(Account githubUser)
        {
            if (string.IsNullOrWhiteSpace(githubUser?.Name))
            {
                _logger.Terse(
                    "GitHub user name missing from profile, falling back to .gitconfig");
                return null;
            }
            if (string.IsNullOrWhiteSpace(githubUser?.Email))
            {
                _logger.Terse(
                    "GitHub public email missing from profile, falling back to .gitconfig");
                return null;
            }

            return new Identity(githubUser.Name, githubUser.Email);
        }
    }
}
