using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Creators
{
    public class RepositoryFilterCreator : ICreate<IRepositoryFilter>
    {
        private readonly ICreate<IGitHub> _githubCreator;
        private readonly INuKeeperLogger _logger;

        public RepositoryFilterCreator(ICreate<IGitHub> githubCreator, INuKeeperLogger logger)
        {
            _githubCreator = githubCreator;
            _logger = logger;
        }

        public IRepositoryFilter Create(SettingsContainer settings)
        {
            return new RepositoryFilter(_githubCreator.Create(settings), _logger);
        }
    }
}
