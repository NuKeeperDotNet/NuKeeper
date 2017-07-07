using System.Collections.Generic;
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
    }
}