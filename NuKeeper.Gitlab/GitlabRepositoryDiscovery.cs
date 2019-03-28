using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.Gitlab
{
    public class GitlabRepositoryDiscovery : IRepositoryDiscovery
    {
        private readonly INuKeeperLogger _nuKeeperLogger;
        private readonly ICollaborationPlatform _collaborationPlatform;

        public GitlabRepositoryDiscovery(INuKeeperLogger nuKeeperLogger, ICollaborationPlatform collaborationPlatform)
        {
            _nuKeeperLogger = nuKeeperLogger;
            _collaborationPlatform = collaborationPlatform;

            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<RepositorySettings>> GetRepositories(SourceControlServerSettings settings)
        {
            throw new System.NotImplementedException();
        }
    }
}
