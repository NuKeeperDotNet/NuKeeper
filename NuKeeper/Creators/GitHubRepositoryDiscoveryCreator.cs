using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Github;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Creators
{
    public class GitHubRepositoryDiscoveryCreator : ICreate<IGithubRepositoryDiscovery>
    {
        private readonly INuKeeperLogger _logger;
        private readonly ICreate<IGithub> _githubCreator;

        public GitHubRepositoryDiscoveryCreator(ICreate<IGithub> githubCreator, INuKeeperLogger logger)
        {
            _logger = logger;
            _githubCreator = githubCreator;
        }

        public IGithubRepositoryDiscovery Create(SettingsContainer settings)
        {
            return new GithubRepositoryDiscovery(_githubCreator.Create(settings), settings.ModalSettings, _logger);
        }
    }
}
