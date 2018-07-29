using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Creators
{
    public class GitHubRepositoryDiscoveryCreator : ICreate<IGitHubRepositoryDiscovery>
    {
        private readonly INuKeeperLogger _logger;
        private readonly ICreate<IGitHub> _githubCreator;

        public GitHubRepositoryDiscoveryCreator(ICreate<IGitHub> githubCreator, INuKeeperLogger logger)
        {
            _logger = logger;
            _githubCreator = githubCreator;
        }

        public IGitHubRepositoryDiscovery Create(SettingsContainer settings)
        {
            return new GitHubRepositoryDiscovery(_githubCreator.Create(settings), settings.ModalSettings, _logger);
        }
    }
}
