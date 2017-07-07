using System.Collections.Generic;
using System.Linq;
using NuKeeper.Configuration;
using NuKeeper.Github;

namespace NuKeeper
{
    public class GithubRepositoryDiscovery
    {
        private GithubClient _github;

        public GithubRepositoryDiscovery(GithubClient github)
        {
            _github = github;
        }

        public IEnumerable<RepositoryModeSettings> FromOrganisation(OrganisationModeSettings settingsOrganisation)
        {
            return new List<RepositoryModeSettings>();
        }

        public IEnumerable<RepositoryModeSettings> GetRepositories(Settings settings)
        {
            if (settings.Mode == Settings.OrganisationMode)
            {
                return FromOrganisation(settings.Organisation);
            }
            if (settings.Mode == Settings.RepositoryMode)
            {
                return new[] { settings.Repository };
            }
            return Enumerable.Empty<RepositoryModeSettings>();
        }
    }
}