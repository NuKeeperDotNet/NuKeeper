using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NuKeeper.Abstract;
using NuKeeper.Abstract.Configuration;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Github.Engine
{
    public class GitHubRepositoryDiscovery : IGitHubRepositoryDiscovery
    {
        private readonly INuKeeperLogger _logger;

        public GitHubRepositoryDiscovery(
            INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<IRepositorySettings>> GetRepositories(
            IClient gitHub, SourceControlServerSettings settings)
        {
            switch (settings.Scope)
            {
                case ServerScope.Global:
                    return await ForAllOrgs(gitHub, settings);

                case ServerScope.Organisation:
                    return await FromOrganisation(gitHub, settings.OrganisationName, settings);

                case ServerScope.Repository:
                    return new[] { settings.Repository };

                default:
                    _logger.Error($"Unknown Server Scope {settings.Scope}");
                    return Enumerable.Empty<RepositorySettings>();
            }
        }

        private async Task<IReadOnlyCollection<RepositorySettings>> ForAllOrgs(
            IClient gitHubClient, SourceControlServerSettings settings)
        {
            var allOrgs = await gitHubClient.GetOrganizations();

            var allRepos = new List<RepositorySettings>();

            foreach (var org in allOrgs)
            {
                var repos = await FromOrganisation(gitHubClient, org.Name ?? org.Login, settings);
                allRepos.AddRange(repos);
            }

            return allRepos;
        }

        private async Task<IReadOnlyCollection<RepositorySettings>> FromOrganisation(
            IClient gitHubClient, string organisationName, SourceControlServerSettings settings)
        {
            var allOrgRepos = await gitHubClient.GetRepositoriesForOrganisation(organisationName);

            var usableRepos = allOrgRepos
                .Where(r => MatchesIncludeExclude(r, settings))
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

        private static bool MatchesIncludeExclude(IRepository repo, SourceControlServerSettings settings)
        {
            return
                MatchesInclude(settings.IncludeRepos, repo)
                && !MatchesExclude(settings.ExcludeRepos, repo);
        }

        private static bool MatchesInclude(Regex regex, IRepository repo)
        {
            return regex == null || regex.IsMatch(repo.Name);
        }

        private static bool MatchesExclude(Regex regex, IRepository repo)
        {
            return regex != null && regex.IsMatch(repo.Name);
        }

        private static bool RepoIsModifiable(IRepository repo)
        {
            return
                !repo.Archived &&
                repo.Permissions.Pull;
        }
    }
}
