using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Github;
using NuKeeper.Inspection.Logging;
using Octokit;

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
                case RunMode.Global:
                    return await ForAllOrgs();

                case RunMode.Organisation:
                    return await FromOrganisation(_settings.OrganisationName);

                case RunMode.Repository:
                    return new[] { _settings.Repository };

                default:
                    return Enumerable.Empty<RepositorySettings>();
            }
        }

        private async Task<IReadOnlyCollection<RepositorySettings>> ForAllOrgs()
        {
            var allOrgs = await _github.GetOrganizations();

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
            var allOrgRepos = await _github.GetRepositoriesForOrganisation(organisationName);

            var usableRepos = allOrgRepos
                .Where(RepoIsModifiable)
                .ToList();

            if (allOrgRepos.Count > usableRepos.Count)
            {
                _logger.Verbose($"Can pull from {usableRepos.Count} repos out of {allOrgRepos.Count}");
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
