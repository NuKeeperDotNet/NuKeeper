using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.Engine
{
    public interface IGitHubRepositoryDiscovery
    {
        Task<IEnumerable<RepositorySettings>> GetRepositories(ICollaborationPlatform github, SourceControlServerSettings settings);
    }
}
