using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstract.Configuration;

namespace NuKeeper.AzureDevOps.Engine
{
    public interface IAzureDevopsRepositoryDiscovery
    {
        Task<IReadOnlyCollection<RepositorySettings>> GetRepositories(IAzureDevOpsClient github, SourceControlServerSettings sourceControlServerSettings);
    }
}
