using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Engine
{
    public interface IGitHubRepositoryDiscovery
    {
        Task<IEnumerable<RepositorySettings>> GetRepositories(ICollaborationPlatform github, SourceControlServerSettings settings);
    }
}
