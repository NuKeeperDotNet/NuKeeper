using System;
using System.Threading.Tasks;
using NuKeeper.Abstract;
using NuKeeper.Abstract.Configuration;
using NuKeeper.Abstract.Engine;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Github.Engine
{
    public class GithubForkFinder: IForkFinder
    {
        private readonly IClient _gitHubClient;
        private readonly INuKeeperLogger _logger;

        public GithubForkFinder(IClient gitHubClient, INuKeeperLogger logger)
        {
            _gitHubClient = gitHubClient;
            _logger = logger;
        }

        public async Task<IForkData> FindPushFork(ForkMode forkMode, string userName, ForkData fallbackFork)
        {
            _logger.Detailed($"FindPushFork. Fork Mode is {forkMode}");

            switch (forkMode)
            {
                case ForkMode.PreferFork:
                    return await FindUserForkOrUpstream(forkMode, userName, fallbackFork);

                case ForkMode.PreferSingleRepository:
                    return await FindUpstreamRepoOrUserFork(forkMode, userName, fallbackFork);

                case ForkMode.SingleRepositoryOnly:
                    return await FindUpstreamRepoOnly(forkMode, fallbackFork);

                default:
                    throw new ArgumentOutOfRangeException($"Unknown fork mode: {forkMode}");
            }
        }

        private async Task<IForkData> FindUserForkOrUpstream(ForkMode forkMode, string userName, ForkData pullFork)
        {
            var userFork = await TryFindUserFork(userName, pullFork);
            if (userFork != null)
            {
                return userFork;
            }

            // as a fallback, we want to pull and push from the same origin repo.
            var canUseOriginRepo = await IsPushableRepo(pullFork);
            if (canUseOriginRepo)
            {
                _logger.Normal($"No fork for user {userName}. Using upstream fork for user {pullFork.Owner} at {pullFork.Uri}");
                return pullFork;
            }

            NoPushableForkFound(forkMode, pullFork.Name);
            return null;
        }

        private async Task<IForkData> FindUpstreamRepoOrUserFork(ForkMode forkMode, string userName, ForkData pullFork)
        {
            // prefer to pull and push from the same origin repo.
            var canUseOriginRepo = await IsPushableRepo(pullFork);
            if (canUseOriginRepo)
            {
                _logger.Normal($"Using upstream fork as push, for user {pullFork.Owner} at {pullFork.Uri}");
                return pullFork;
            }

            // fall back to trying a fork
            var userFork = await TryFindUserFork(userName, pullFork);
            if (userFork != null)
            {
                return userFork;
            }

            NoPushableForkFound(forkMode, pullFork.Name);
            return null;
        }

        private async Task<ForkData> FindUpstreamRepoOnly(ForkMode forkMode, ForkData pullFork)
        {
            // Only want to pull and push from the same origin repo.
            var canUseOriginRepo = await IsPushableRepo(pullFork);
            if (canUseOriginRepo)
            {
                _logger.Normal($"Using upstream fork as push, for user {pullFork.Owner} at {pullFork.Uri}");
                return pullFork;
            }

            NoPushableForkFound(forkMode, pullFork.Name);
            return null;
        }

        private void NoPushableForkFound(ForkMode forkMode, string name)
        {
            _logger.Error($"No pushable fork found for {name} in mode {forkMode}");
        }

        private async Task<bool> IsPushableRepo(ForkData originFork)
        {
            var originRepo = await _gitHubClient.GetUserRepository(originFork.Owner, originFork.Name);
            return originRepo != null && originRepo.Permissions.Push;
        }

        private async Task<IForkData> TryFindUserFork(string userName, ForkData originFork)
        {
            var userFork = await _gitHubClient.GetUserRepository(userName, originFork.Name);
            if (userFork != null)
            {
                if (RepoIsForkOf(userFork, originFork.Uri.ToString()) && userFork.Permissions.Push)
                {
                    // the user has a pushable fork
                    return RepositoryToForkData(userFork);
                }

                // the user has a repo of that name, but it can't be used.
                // Don't try to create it
                _logger.Normal($"User '{userName}' fork of '{originFork.Name}' exists but is unsuitable.");
                return null;
            }

            // no user fork exists, try and create it as a fork of the main repo
            var newFork = await _gitHubClient.MakeUserFork(originFork.Owner, originFork.Name);
            if (newFork != null)
            {
                return RepositoryToForkData(newFork);
            }

            return null;
        }

        private static bool RepoIsForkOf(IRepository userRepo, string parentUrl)
        {
            if (! userRepo.Fork)
            {
                return false;
            }

            return UrlIsMatch(userRepo.Parent?.CloneUrl.ToString(), parentUrl)
                || UrlIsMatch(userRepo.Parent?.HtmlUrl.ToString(), parentUrl);
        }

        private static bool UrlIsMatch(string test, string expected)
        {
            return !string.IsNullOrWhiteSpace(test) &&
                string.Equals(test, expected, StringComparison.OrdinalIgnoreCase);
        }

        private static IForkData RepositoryToForkData(IRepository repo)
        {
            return new ForkData(repo.CloneUrl, repo.Owner.Login, repo.Name);
        }
    }
}
