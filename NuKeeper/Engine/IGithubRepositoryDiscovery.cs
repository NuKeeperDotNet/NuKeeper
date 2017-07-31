using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Configuration;

namespace NuKeeper.Engine
{
    public interface IGithubRepositoryDiscovery
    {
        Task<IEnumerable<RepositoryModeSettings>> FromOrganisation(OrganisationModeSettings organisation);
        Task<IEnumerable<RepositoryModeSettings>> GetRepositories();
    }
}