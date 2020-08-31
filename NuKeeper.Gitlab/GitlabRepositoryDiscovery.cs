using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.Gitlab
{
    public class GitlabRepositoryDiscovery : IRepositoryDiscovery
    {
        private readonly INuKeeperLogger _logger;
        private readonly ICollaborationPlatform _collaborationPlatform;

        public GitlabRepositoryDiscovery(INuKeeperLogger logger, ICollaborationPlatform collaborationPlatform)
        {
            _logger = logger;
            _collaborationPlatform = collaborationPlatform;
        }

        public Task<IEnumerable<RepositorySettings>> GetRepositories(SourceControlServerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            switch (settings.Scope)
            {
                case ServerScope.Global:
                case ServerScope.Organisation:
                    _logger.Error($"{settings.Scope} not yet implemented");
                    throw new NotImplementedException();

                case ServerScope.Repository:
                    IEnumerable<RepositorySettings> repositorySettings = new[] { settings.Repository };
                    return Task.FromResult(repositorySettings);

                default:
                    _logger.Error($"Unknown Server Scope {settings.Scope}");
                    throw new NotImplementedException();
            }
        }
    }
}
