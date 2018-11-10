using System;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.DTOs;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsForkFinder : IForkFinder
    {
        private readonly ICollaborationPlatform _collaborationPlatform;
        private readonly INuKeeperLogger _logger;

        public AzureDevOpsForkFinder(ICollaborationPlatform collaborationPlatform, INuKeeperLogger logger)
        {
            _collaborationPlatform = collaborationPlatform;
            _logger = logger;
        }

        public async Task<ForkData> FindPushFork(ForkMode forkMode, string userName, ForkData fallbackFork)
        {
            _logger.Detailed($"FindPushFork. Fork Mode is {forkMode}");

            switch (forkMode)
            {
                case ForkMode.PreferFork:
                case ForkMode.PreferSingleRepository:
                    _logger.Error($"{forkMode} not yet implemented");
                    throw new NotImplementedException();

                case ForkMode.SingleRepositoryOnly:
                    return await FindUpstreamRepoOnly(forkMode, fallbackFork);

                default:
                    throw new ArgumentOutOfRangeException($"Unknown fork mode: {forkMode}");
            }
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
            var originRepo = await _collaborationPlatform.GetUserRepository(originFork.Owner, originFork.Name);
            return originRepo != null && originRepo.UserPermissions.Push;
        }
    }
}
