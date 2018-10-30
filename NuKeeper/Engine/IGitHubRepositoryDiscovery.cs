using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.GitHub;

namespace NuKeeper.Engine
{
    public interface IGitHubRepositoryDiscovery
    {
        Task<IEnumerable<RepositorySettings>> GetRepositories(IGitHub github, SourceControlServerSettings settings);
    }
}
