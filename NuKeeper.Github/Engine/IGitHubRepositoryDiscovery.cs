using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Github.Engine
{
    public interface IGitHubRepositoryDiscovery
    {
        Task<IEnumerable<IRepositorySettings>> GetRepositories(IClient github, SourceControlServerSettings settings);
    }
}
