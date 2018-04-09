using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Github;
using NuKeeper.Types.Logging;

namespace NuKeeper.Engine
{
    public class GithubRepositoryDiscovery : IGithubRepositoryDiscovery
    {
        private readonly IGithub _github;
        private readonly ModalSettings _settings;
        private readonly INuKeeperLogger _logger;

        public GithubRepositoryDiscovery(
            IGithub github, 
            ModalSettings settings,
            INuKeeperLogger logger)
        {
            _github = github;
            _settings = settings;
            _logger = logger;
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
            var allOrgRepos = await _github.GetRepositoriesForOrganisation(organisationName);

            var usableRepos = allOrgRepos
                .Where(r => r.Permissions.Pull)
                .ToList();

            if (allOrgRepos.Count > usableRepos.Count)
            {
                _logger.Verbose($"Can pull from {usableRepos.Count} repos out of {allOrgRepos.Count}");
            }

            return usableRepos.Select(r => new RepositorySettings(r));
        }
    }
}
