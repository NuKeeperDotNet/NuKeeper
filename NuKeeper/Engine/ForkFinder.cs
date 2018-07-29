using System;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Engine
{
    public class ForkFinder: IForkFinder
    {
        private readonly IGitHub _gitHub;
        private readonly INuKeeperLogger _logger;
        private readonly ForkMode _forkMode;

        public ForkFinder(IGitHub gitHub, UserSettings settings, INuKeeperLogger logger)
        {
            _gitHub = gitHub;
            _forkMode = settings.ForkMode;
            _logger = logger;
        }

        public async Task<ForkData> FindPushFork(string userName, ForkData fallbackFork)
        {
            _logger.Verbose($"FindPushFork. Fork Mode is {_forkMode}");

            switch (_forkMode)
            {
                case ForkMode.PreferFork:
                    return await FindUserForkOrUpstream(userName, fallbackFork);

                case ForkMode.PreferSingleRepository:
                    return await FindUpstreamRepoOrUserFork(userName, fallbackFork);

                case ForkMode.SingleRepositoryOnly:
                    return await FindUpstreamRepoOnly(fallbackFork);

                default:
                    throw new Exception($"Unknown fork mode: {_forkMode}");
            }
        }

        private async Task<ForkData> FindUserForkOrUpstream(string userName, ForkData pullFork)
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
                _logger.Info($"No fork for user {userName}. Using upstream fork for user {pullFork.Owner} at {pullFork.Uri}");
                return pullFork;
            }

            NoPushableForkFound(pullFork.Name);
            return null;
        }

        private async Task<ForkData> FindUpstreamRepoOrUserFork(string userName, ForkData pullFork)
        {
            // prefer to pull and push from the same origin repo.
            var canUseOriginRepo = await IsPushableRepo(pullFork);
            if (canUseOriginRepo)
            {
                _logger.Info($"Using upstream fork as push, for user {pullFork.Owner} at {pullFork.Uri}");
                return pullFork;
            }

            // fall back to trying a fork
            var userFork = await TryFindUserFork(userName, pullFork);
            if (userFork != null)
            {
                return userFork;
            }

            NoPushableForkFound(pullFork.Name);
            return null;
        }

        private async Task<ForkData> FindUpstreamRepoOnly(ForkData pullFork)
        {
            // Only want to pull and push from the same origin repo.
            var canUseOriginRepo = await IsPushableRepo(pullFork);
            if (canUseOriginRepo)
            {
                _logger.Info($"Using upstream fork as push, for user {pullFork.Owner} at {pullFork.Uri}");
                return pullFork;
            }

            NoPushableForkFound(pullFork.Name);
            return null;
        }

        private void NoPushableForkFound(string name)
        {
            _logger.Error($"No pushable fork found for {name} in mode {_forkMode}");
        }

        private async Task<bool> IsPushableRepo(ForkData originFork)
        {
            var originRepo = await _gitHub.GetUserRepository(originFork.Owner, originFork.Name);
            return originRepo != null && originRepo.Permissions.Push;
        }

        private async Task<ForkData> TryFindUserFork(string userName, ForkData originFork)
        {
            var userFork = await _gitHub.GetUserRepository(userName, originFork.Name);
            if (userFork != null)
            {
                if (RepoIsForkOf(userFork, originFork.Uri.ToString()) && userFork.Permissions.Push)
                {
                    // the user has a pushable fork
                    return RepositoryToForkData(userFork);
                }

                // the user has a repo of that name, but it can't be used. 
                // Don't try to create it
                _logger.Info($"User '{userName}' fork of '{originFork.Name}' exists but is unsuitable.");
                return null;
            }

            // no user fork exists, try and create it as a fork of the main repo
            var newFork = await _gitHub.MakeUserFork(originFork.Owner, originFork.Name);
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

            return UrlIsMatch(userRepo.Parent?.CloneUrl, parentUrl)
                || UrlIsMatch(userRepo.Parent?.HtmlUrl, parentUrl);
        }

        private static bool UrlIsMatch(string test, string expected)
        {
            return !string.IsNullOrWhiteSpace(test) &&
                string.Equals(test, expected, StringComparison.OrdinalIgnoreCase);
        }

        private static ForkData RepositoryToForkData(IRepository repo)
        {
            return new ForkData(new Uri(repo.CloneUrl), repo.Owner.Login, repo.Name);
        }
    }
}
