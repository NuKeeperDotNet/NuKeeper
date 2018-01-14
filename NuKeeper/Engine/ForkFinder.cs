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
                if (IsForkOf(userFork, fallbackFork.Uri.ToString())  && userFork.Permissions.Push)
                {
                    return RepositoryToForkData(userFork);
                }

                // it exits. but can't be used. Don't try to create it
                _logger.Info($"User fork exists but is unsuitable. Using fallback push fork for user {fallbackFork.Owner} at {fallbackFork.Uri}");
                return fallbackFork;
            }

            // try and make a fork
            var newFork = await _github.MakeUserFork(fallbackFork.Owner, repositoryName);
            if (newFork != null)
            {
                return RepositoryToForkData(newFork);
            }

            // we have pull and push from the same place as a fallback
            _logger.Info($"Cannot make user fork. Using fallback push fork for user {fallbackFork.Owner} at {fallbackFork.Uri}");
            return fallbackFork;
        }

        private static bool IsForkOf(Repository fork, string parentUri)
        {
            return fork.Fork &&
                   !string.IsNullOrWhiteSpace(fork.Parent?.HtmlUrl) &&
                   string.Equals(fork.Parent.HtmlUrl, parentUri, StringComparison.OrdinalIgnoreCase);
        }

        private static ForkData RepositoryToForkData(Octokit.Repository repo)
        {
            return new ForkData(new Uri(repo.HtmlUrl), repo.Owner.Login, repo.Name);
        }
    }
}
