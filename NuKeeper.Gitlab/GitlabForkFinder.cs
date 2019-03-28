using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.Gitlab
{
    public class GitlabForkFinder : IForkFinder
    {
        private readonly ICollaborationPlatform _collaborationPlatform;
        private readonly INuKeeperLogger _nuKeeperLogger;

        public GitlabForkFinder(ICollaborationPlatform collaborationPlatform, INuKeeperLogger nuKeeperLogger)
        {
            _collaborationPlatform = collaborationPlatform;
            _nuKeeperLogger = nuKeeperLogger;

            throw new System.NotImplementedException();
        }

        public Task<ForkData> FindPushFork(string userName, ForkData fallbackFork)
        {
            throw new System.NotImplementedException();
        }
    }
}
