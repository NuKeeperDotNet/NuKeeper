using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.GitHub
{
    public class GitHubRepositoryDiscovery : IRepositoryDiscovery
    {
        private readonly INuKeeperLogger _logger;
        private readonly ICollaborationPlatform _collaborationPlatform;

        public GitHubRepositoryDiscovery(INuKeeperLogger logger, ICollaborationPlatform collaborationPlatform)
        {
            _logger = logger;
            _collaborationPlatform = collaborationPlatform;
        }

        public async Task<IEnumerable<RepositorySettings>> GetRepositories(SourceControlServerSettings settings)
        {
            switch (settings.Scope)
            {
                case ServerScope.Global:
                    return await ForAllOrgs(settings);

                case ServerScope.Organisation:
                    return await FromOrganisation(settings.OrganisationName, settings);

                case ServerScope.Repository:
                    return new[] { settings.Repository };

                default:
                    _logger.Error($"Unknown Server Scope {settings.Scope}");
                    return Enumerable.Empty<RepositorySettings>();
            }
        }

        private async Task<IReadOnlyCollection<RepositorySettings>> ForAllOrgs(SourceControlServerSettings settings)
        {
            var allOrgs = await _collaborationPlatform.GetOrganizations();

            var allRepos = new List<RepositorySettings>();

            foreach (var org in allOrgs)
            {
                var repos = await FromOrganisation(org.Name, settings);
                allRepos.AddRange(repos);
            }

            return allRepos;
        }

        private async Task<IReadOnlyCollection<RepositorySettings>> FromOrganisation(string organisationName, SourceControlServerSettings settings)
        {
            var allOrgRepos = await _collaborationPlatform.GetRepositoriesForOrganisation(organisationName);

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

        private static bool MatchesIncludeExclude(Repository repo, SourceControlServerSettings settings)
        {
            return RegexMatch.IncludeExclude(repo.Name, settings.IncludeRepos, settings.ExcludeRepos);
        }

        private static bool RepoIsModifiable(Repository repo)
        {
            return
                !repo.Archived &&
                repo.UserPermissions.Pull;
        }
    }
}
