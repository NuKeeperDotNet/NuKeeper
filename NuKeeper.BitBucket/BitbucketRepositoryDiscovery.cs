using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.BitBucket
{
    public class BitbucketRepositoryDiscovery : IRepositoryDiscovery
    {
        private readonly INuKeeperLogger _logger;

        public BitbucketRepositoryDiscovery(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public Task<IEnumerable<RepositorySettings>> GetRepositories(SourceControlServerSettings settings)
        {
            switch (settings.Scope)
            {
                case ServerScope.Global:
                case ServerScope.Organisation:
                    _logger.Error($"{settings.Scope} not yet implemented");
                    throw new NotImplementedException();

                case ServerScope.Repository:
                    return Task.FromResult(new List<RepositorySettings> { settings.Repository }.AsEnumerable());

                default:
                    _logger.Error($"Unknown Server Scope {settings.Scope}");
                    return Task.FromResult(Enumerable.Empty<RepositorySettings>());
            }
        }
    }
}
