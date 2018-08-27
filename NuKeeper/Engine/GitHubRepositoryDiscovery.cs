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
        private readonly INuKeeperLogger _logger;

        public GitHubRepositoryDiscovery(
            INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<RepositorySettings>> GetRepositories(
            IGitHub gitHub, SourceControlServerSettings settings)
        {
            switch (settings.Scope)
            {
                case ServerScope.Global:
                    return await ForAllOrgs(gitHub);

                case ServerScope.Organisation:
                    return await FromOrganisation(gitHub, settings.OrganisationName);

                case ServerScope.Repository:
                    return new[] { settings.Repository };

                default:
                    _logger.Error($"Unknown Server Scope {settings.Scope}");
                    return Enumerable.Empty<RepositorySettings>();
            }
        }

        private async Task<IReadOnlyCollection<RepositorySettings>> ForAllOrgs(IGitHub gitHub)
        {
            var allOrgs = await gitHub.GetOrganizations();

            var allRepos = new List<RepositorySettings>();

            foreach (var org in allOrgs)
            {
                var repos = await FromOrganisation(gitHub, org.Name ?? org.Login);
                allRepos.AddRange(repos);
            }

            return allRepos;
        }

        private async Task<IReadOnlyCollection<RepositorySettings>> FromOrganisation(
            IGitHub gitHub, string organisationName)
        {
            var allOrgRepos = await gitHub.GetRepositoriesForOrganisation(organisationName);

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
