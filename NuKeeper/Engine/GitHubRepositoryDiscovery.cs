using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;

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
            ICollaborationPlatform collaborationPlatform, SourceControlServerSettings settings)
        {
            switch (settings.Scope)
            {
                case ServerScope.Global:
                    return await ForAllOrgs(collaborationPlatform, settings);

                case ServerScope.Organisation:
                    return await FromOrganisation(collaborationPlatform, settings.OrganisationName, settings);

                case ServerScope.Repository:
                    return new[] { settings.Repository };

                default:
                    _logger.Error($"Unknown Server Scope {settings.Scope}");
                    return Enumerable.Empty<RepositorySettings>();
            }
        }

        private async Task<IReadOnlyCollection<RepositorySettings>> ForAllOrgs(
            ICollaborationPlatform collaborationPlatform, SourceControlServerSettings settings)
        {
            var allOrgs = await collaborationPlatform.GetOrganizations();

            var allRepos = new List<RepositorySettings>();

            foreach (var org in allOrgs)
            {
                var repos = await FromOrganisation(collaborationPlatform, org.Name ?? org.Login, settings);
                allRepos.AddRange(repos);
            }

            return allRepos;
        }

        private async Task<IReadOnlyCollection<RepositorySettings>> FromOrganisation(
            ICollaborationPlatform collaborationPlatform, string organisationName, SourceControlServerSettings settings)
        {
            var allOrgRepos = await collaborationPlatform.GetRepositoriesForOrganisation(organisationName);

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

        private static bool MatchesIncludeExclude(Abstractions.DTOs.Repository repo, SourceControlServerSettings settings)
        {
            return
                MatchesInclude(settings.IncludeRepos, repo)
                && !MatchesExclude(settings.ExcludeRepos, repo);
        }

        private static bool MatchesInclude(Regex regex, Abstractions.DTOs.Repository repo)
        {
            return regex == null || regex.IsMatch(repo.Name);
        }

        private static bool MatchesExclude(Regex regex, Abstractions.DTOs.Repository repo)
        {
            return regex != null && regex.IsMatch(repo.Name);
        }

        private static bool RepoIsModifiable(Abstractions.DTOs.Repository repo)
        {
            return
                ! repo.Archived &&
                repo.UserPermissions.Pull;
        }
    }
}
