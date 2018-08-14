using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Logging;
using Octokit;

namespace NuKeeper.Engine
{
    public class GitHubRepositoryDiscovery : IGitHubRepositoryDiscovery
    {
        private readonly IGitHub _gitHub;
        private readonly ModalSettings _settings;
        private readonly INuKeeperLogger _logger;

        public GitHubRepositoryDiscovery(
            IGitHub gitHub, 
            ModalSettings settings,
            INuKeeperLogger logger)
        {
            _gitHub = gitHub;
            _settings = settings;
            _logger = logger;
        }

        public async Task<IEnumerable<RepositorySettings>> GetRepositories(GithubScope scope)
        {
            switch (scope)
            {
                case GithubScope.Global:
                    return await ForAllOrgs();

                case GithubScope.Organisation:
                    return await FromOrganisation(_settings.OrganisationName);

                case GithubScope.Repository:
                    return new[] { _settings.Repository };

                default:
                    return Enumerable.Empty<RepositorySettings>();
            }
        }

        private async Task<IReadOnlyCollection<RepositorySettings>> ForAllOrgs()
        {
            var allOrgs = await _gitHub.GetOrganizations();

            var allRepos = new List<RepositorySettings>();

            foreach (var org in allOrgs)
            {
                var repos = await FromOrganisation(org.Name ?? org.Login);
                allRepos.AddRange(repos);
            }

            return allRepos;
        }

        private async Task<IReadOnlyCollection<RepositorySettings>> FromOrganisation(string organisationName)
        {
            var allOrgRepos = await _gitHub.GetRepositoriesForOrganisation(organisationName);

            var usableRepos = allOrgRepos
                .Where(RepoIsModifiable)
                .ToList();

            if (allOrgRepos.Count > usableRepos.Count)
            {
                _logger.Detailed($"Can pull from {usableRepos.Count} repos out of {allOrgRepos.Count}");
            }

            return usableRepos
                .Select(r => new RepositorySettings(r))
                .ToList();
        }

        private static bool RepoIsModifiable(Repository repo)
        {
            return
                ! repo.Archived &&
                repo.Permissions.Pull;
        }
    }
}
