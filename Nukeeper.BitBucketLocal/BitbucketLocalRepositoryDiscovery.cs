using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.BitBucketLocal
{
    public class BitbucketLocalRepositoryDiscovery : IRepositoryDiscovery
    {
        private readonly INuKeeperLogger _logger;
        private readonly ICollaborationPlatform _collaborationPlatform;
        private CollaborationPlatformSettings _setting;

        public BitbucketLocalRepositoryDiscovery(INuKeeperLogger logger, ICollaborationPlatform collaborationPlatform, CollaborationPlatformSettings settings)
        {
            _logger = logger;
            _collaborationPlatform = collaborationPlatform;
            _setting = settings;
        }

        public async Task<IEnumerable<RepositorySettings>> GetRepositories(SourceControlServerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            switch (settings.Scope)
            {
                case ServerScope.Global:
                    _logger.Error($"{settings.Scope} not yet implemented");
                    throw new NotImplementedException();

                case ServerScope.Organisation:
                    return await FromOrganisation(settings.OrganisationName, settings);

                case ServerScope.Repository:
                    return await Task.FromResult(new List<RepositorySettings> { settings.Repository }.AsEnumerable());

                default:
                    _logger.Error($"Unknown Server Scope {settings.Scope}");
                    return await Task.FromResult(Enumerable.Empty<RepositorySettings>());
            }
        }



        private async Task<IReadOnlyCollection<RepositorySettings>> FromOrganisation(string organisationName, SourceControlServerSettings settings)
        {
            var allOrgRepos = await _collaborationPlatform.GetRepositoriesForOrganisation(organisationName);

            var usableRepos = allOrgRepos
                .Where(r => MatchesIncludeExclude(r, settings))
                .ToList();

            if (allOrgRepos.Count > usableRepos.Count)
            {
                _logger.Detailed($"Can pull from {usableRepos.Count} repos out of {allOrgRepos.Count}");
            }

            return usableRepos
                .Select(r => new RepositorySettings
                {
                    ApiUri = _setting.BaseApiUrl,
                    RepositoryUri = r.CloneUrl,
                    RepositoryName = r.Name,
                    RepositoryOwner = organisationName
                }).ToList();
        }

        private static bool MatchesIncludeExclude(Repository repo, SourceControlServerSettings settings)
        {
            return RegexMatch.IncludeExclude(repo.Name, settings.IncludeRepos, settings.ExcludeRepos);
        }
    }
}
