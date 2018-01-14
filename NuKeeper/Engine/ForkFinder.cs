using System;
using System.Threading.Tasks;
using NuKeeper.Github;
using NuKeeper.Logging;
using Octokit;

namespace NuKeeper.Engine
{

    public class ForkFinder: IForkFinder
    {
        private readonly IGithub _github;
        private readonly INuKeeperLogger _logger;

        public ForkFinder(IGithub github,INuKeeperLogger logger)
        {
            _github = github;
            _logger = logger;
        }

        public async Task<ForkData> PushFork(string userName, string repositoryName, ForkData fallbackFork)
        {
            var userFork = await _github.GetUserRepository(userName, repositoryName);
            if (userFork != null)
            {
                if (RepoIsForkOf(userFork, fallbackFork.Uri.ToString())  && userFork.Permissions.Push)
                {
                    // the user has a suitable fork
                    return RepositoryToForkData(userFork);
                }

                // the user has a repo of that name, but it can't be used. 
                // Don't try to create it
                _logger.Info($"User fork exists but is unsuitable. Using fallback push fork for user {fallbackFork.Owner} at {fallbackFork.Uri}");
                return fallbackFork;
            }

            // try and create a fork of the main repo
            var newFork = await _github.MakeUserFork(fallbackFork.Owner, repositoryName);
            if (newFork != null)
            {
                return RepositoryToForkData(newFork);
            }

            // as a fallback, we pull and push from the same repo
            _logger.Info($"Cannot make user fork. Using fallback push fork for user {fallbackFork.Owner} at {fallbackFork.Uri}");
            return fallbackFork;
        }

        private static bool RepoIsForkOf(Repository userRepo, string parentUri)
        {
            return userRepo.Fork &&
                   !string.IsNullOrWhiteSpace(userRepo.Parent?.HtmlUrl) &&
                   string.Equals(userRepo.Parent.HtmlUrl, parentUri, StringComparison.OrdinalIgnoreCase);
        }

        private static ForkData RepositoryToForkData(Repository repo)
        {
            return new ForkData(new Uri(repo.HtmlUrl), repo.Owner.Login, repo.Name);
        }
    }
}
