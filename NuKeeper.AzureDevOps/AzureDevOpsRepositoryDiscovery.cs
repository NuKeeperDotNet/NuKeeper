using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsRepositoryDiscovery : IRepositoryDiscovery
    {
        private readonly INuKeeperLogger _logger;
        private readonly string _token;

        public AzureDevOpsRepositoryDiscovery(INuKeeperLogger logger, string token)
        {
            _logger = logger;
            _token = token;
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
                    //Workaround for https://github.com/libgit2/libgit2sharp/issues/1596
                    settings.Repository.RepositoryUri = new Uri(settings.Repository.RepositoryUri.ToString().Replace("--PasswordToReplace--", _token));
                    return Task.FromResult(new List<RepositorySettings> { settings.Repository }.AsEnumerable());

                default:
                    _logger.Error($"Unknown Server Scope {settings.Scope}");
                    return Task.FromResult(Enumerable.Empty<RepositorySettings>());
            }
        }
    }
}
