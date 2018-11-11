using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface IRepositoryDiscovery
    {
        Task<IEnumerable<RepositorySettings>> GetRepositories(SourceControlServerSettings settings);
    }
}
