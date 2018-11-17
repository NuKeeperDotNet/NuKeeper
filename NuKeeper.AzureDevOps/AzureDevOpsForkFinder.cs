using System;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsForkFinder : IForkFinder
    {
        private readonly ICollaborationPlatform _collaborationPlatform;
        private readonly INuKeeperLogger _logger;
        private readonly ForkMode _forkMode;

        public AzureDevOpsForkFinder(ICollaborationPlatform collaborationPlatform, INuKeeperLogger logger, ForkMode forkMode)
        {
            if (forkMode != ForkMode.SingleRepositoryOnly)
            {
                throw new ArgumentOutOfRangeException(nameof(forkMode), $"{_forkMode} has not yet been implemented");
            }

            _collaborationPlatform = collaborationPlatform;
            _logger = logger;
            _forkMode = forkMode;

            _logger.Detailed($"FindPushFork. Fork Mode is {_forkMode}");
        }

        public async Task<ForkData> FindPushFork(string userName, ForkData fallbackFork)
        {
            return await FindUpstreamRepoOnly(fallbackFork);
        }

        private async Task<ForkData> FindUpstreamRepoOnly(ForkData pullFork)
        {
            // Only want to pull and push from the same origin repo.
            var canUseOriginRepo = await IsPushableRepo(pullFork);
            if (canUseOriginRepo)
            {
                _logger.Normal($"Using upstream fork as push, for project {pullFork.Owner} at {pullFork.Uri}");
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
            var originRepo = await _collaborationPlatform.GetUserRepository(originFork.Owner, originFork.Name);
            return originRepo != null && originRepo.UserPermissions.Push;
        }
    }
}
