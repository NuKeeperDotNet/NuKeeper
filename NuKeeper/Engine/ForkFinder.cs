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
        private readonly ForkMode _forkMode;

        public ForkFinder(IGithub github, UserSettings settings, INuKeeperLogger logger)
        {
            _github = github;
            _forkMode = settings.ForkMode;
            _logger = logger;
        }

        public async Task<ForkData> FindPushFork(string userName, string repositoryName, ForkData fallbackFork)
        {
            switch (_forkMode)
            {
                case ForkMode.PreferFork:
                    return await FindForkOrFallback(userName, repositoryName, fallbackFork);

                case ForkMode.PreferUpstream:
                    return await FindUpstreamRepo(repositoryName, fallbackFork);

                default:
                    throw new Exception($"Unknown fork mode: {_forkMode}");
            }
        }

        private async Task<ForkData> FindForkOrFallback(string userName, string repositoryName, ForkData originFork)
        {
            var userFork = await TryFindUserFork(userName, repositoryName, originFork);
            if (userFork != null)
            {
                return userFork;
            }

            // as a fallback, we want to pull and push from the same origin repo.
            var canUseOriginRepo = await IsPushableRepo(originFork);
            if (canUseOriginRepo)
            {
                _logger.Info($"No fork for user {userName}. Using origin fork for user {originFork.Owner} at {originFork.Uri}");
                return originFork;
            }

            _logger.Error($"No pushable fork found for {repositoryName}");
            throw new Exception($"No pushable fork found for {repositoryName}");
        }

        private async Task<ForkData> FindUpstreamRepo(string repositoryName, ForkData originFork)
        {
            // Only want to pull and push from the same origin repo.
            var canUseOriginRepo = await IsPushableRepo(originFork);
            if (canUseOriginRepo)
            {
                _logger.Info($"Using origin push fork for user {originFork.Owner} at {originFork.Uri}");
                return originFork;
            }

            // fall back to trying a fork?

            _logger.Error($"No pushable fork found for {repositoryName}");
            throw new Exception($"No pushable fork found for {repositoryName}");
        }

        private async Task<bool> IsPushableRepo(ForkData originFork)
        {
            var originRepo = await _github.GetUserRepository(originFork.Owner, originFork.Name);
            return originRepo != null && originRepo.Permissions.Push;
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
