using System;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Github;
using NuKeeper.Logging;
using Octokit;

namespace NuKeeper.Engine
{
    public class ForkFinder: IForkFinder
    {
        private readonly IGithub _github;
        private readonly INuKeeperLogger _logger;
        private ForkMode _forkMode;

        public ForkFinder(IGithub github, UserSettings settings, INuKeeperLogger logger)
        {
            _github = github;
            _forkMode = settings.ForkMode;
            _logger = logger;
        }

        public async Task<ForkData> FindPushFork(string userName, string repositoryName, ForkData fallbackFork)
        {
            if (_forkMode == ForkMode.PreferFork)
            {
                var userFork = await TryFindUserFork(userName, repositoryName, fallbackFork);
                if (userFork != null)
                {
                    return userFork;
                }

            }

            // as a fallback, we want to pull and push from the same origin repo.
            var fallbackRepo = await _github.GetUserRepository(fallbackFork.Owner, repositoryName);
            if (fallbackRepo != null && fallbackRepo.Permissions.Push)
            {
                _logger.Info($"No fork for user {userName}. Using fallback push fork for user {fallbackFork.Owner} at {fallbackFork.Uri}");
                return fallbackFork;
            }

            _logger.Error($"No pushable fork found for {repositoryName}");
            throw new Exception($"No pushable fork found for {repositoryName}");
        }

        private async Task<ForkData> TryFindUserFork(string userName, string repositoryName, ForkData fallbackFork)
        {
            var userFork = await _github.GetUserRepository(userName, repositoryName);
            if (userFork != null)
            {
                if (RepoIsForkOf(userFork, fallbackFork.Uri.ToString()) && userFork.Permissions.Push)
                {
                    // the user has a suitable fork
                    return RepositoryToForkData(userFork);
                }

                // the user has a repo of that name, but it can't be used. 
                // Don't try to create it
                _logger.Info($"User '{userName}' fork of '{repositoryName}' exists but is unsuitable.");
                return null;
            }

            // no user fork exists, try and create it as a fork of the main repo
            var newFork = await _github.MakeUserFork(fallbackFork.Owner, repositoryName);
            if (newFork != null)
            {
                return RepositoryToForkData(newFork);
            }

            return null;
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
