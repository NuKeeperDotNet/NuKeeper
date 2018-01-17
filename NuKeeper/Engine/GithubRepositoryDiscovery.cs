using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Github;

namespace NuKeeper.Engine
{
    public class GithubRepositoryDiscovery : IGithubRepositoryDiscovery
    {
        private readonly IGithub _github;
        private readonly ModalSettings _settings;

        public GithubRepositoryDiscovery(
            IGithub github, 
            ModalSettings settings)
        {
            _github = github;
            _settings = settings;
        }

        public async Task<IEnumerable<RepositorySettings>> GetRepositories()
        {
            switch (_settings.Mode)
            {
                case GithubMode.Organisation:
                    return await FromOrganisation(_settings.OrganisationName);

                case GithubMode.Repository:
                    return new[] { _settings.Repository };

                default:
                    return Enumerable.Empty<RepositorySettings>();
            }
        }

        private async Task<IEnumerable<RepositorySettings>> FromOrganisation(string organisationName)
        {
            var repositories = await _github.GetRepositoriesForOrganisation(organisationName);

            return repositories.Select(r => new RepositorySettings(r));
        }
    }
}