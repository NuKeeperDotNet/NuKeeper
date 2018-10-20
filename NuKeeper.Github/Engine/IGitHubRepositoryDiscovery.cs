using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstract;
using NuKeeper.Abstract.Configuration;

namespace NuKeeper.Github.Engine
{
    public interface IGitHubRepositoryDiscovery
    {
        Task<IEnumerable<IRepositorySettings>> GetRepositories(IClient github, SourceControlServerSettings settings);
    }
}
