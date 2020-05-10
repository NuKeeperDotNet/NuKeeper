using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationModels;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsRepositoryDiscovery : IRepositoryDiscovery
    {
        private readonly INuKeeperLogger _logger;
        private readonly ICollaborationPlatform _collaborationPlatform;
        private readonly string _token;

        public AzureDevOpsRepositoryDiscovery(INuKeeperLogger logger, ICollaborationPlatform collaborationPlatform, string token)
        {
            _logger = logger;
            _collaborationPlatform = collaborationPlatform;
            _token = token;
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
                    
                    settings.Repository.RepositoryUri = PasswordReplacedRepositoryUri(settings.Repository.RepositoryUri);
                    return new List<RepositorySettings> { settings.Repository }.AsEnumerable();

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
                .Select(r => CreateRepositorySettings(organisationName, r.CloneUrl, organisationName, r.Name))
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

        private RepositorySettings CreateRepositorySettings(string org, Uri repositoryUri, string project, string repoName, RemoteInfo remoteInfo = null) => new RepositorySettings
        {
            ApiUri = string.IsNullOrWhiteSpace(org) ? new Uri($"https://dev.azure.com/") : new Uri($"https://dev.azure.com/{org}/"),
            RepositoryUri = PasswordReplacedRepositoryUri(repositoryUri),
            RepositoryName = repoName,
            RepositoryOwner = project,
            RemoteInfo = remoteInfo
        };

        private Uri PasswordReplacedRepositoryUri(Uri repositoryUri)
        {
            //Workaround for https://github.com/libgit2/libgit2sharp/issues/1596
            return new Uri(repositoryUri.ToString().Replace("--PasswordToReplace--", _token));
        }
    }
}
