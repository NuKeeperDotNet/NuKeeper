using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Github;

namespace NuKeeper.Engine
{
    public class GithubRepositoryDiscovery
    {
        private GithubClient _github;
        private Settings _settings;

        public GithubRepositoryDiscovery(GithubClient github, Settings settings)
        {
            _github = github;
            _settings = settings;
        }

        public async Task<IEnumerable<RepositoryModeSettings>> FromOrganisation(OrganisationModeSettings organisation)
        {
            var repositories = await _github.GetRepositoriesForOrganisation(organisation.OrganisationName);

            return repositories.Select(r => new RepositoryModeSettings(r, _settings.GithubApiBase, _settings.GithubToken));
        }

        public async Task<IEnumerable<RepositoryModeSettings>> GetRepositories()
        {
            if (_settings.Mode == Settings.OrganisationMode)
            {
                return await FromOrganisation(_settings.Organisation);
            }
            if (_settings.Mode == Settings.RepositoryMode)
            {
                return new[] { _settings.Repository };
            }
            return Enumerable.Empty<RepositoryModeSettings>();
        }
    }
}