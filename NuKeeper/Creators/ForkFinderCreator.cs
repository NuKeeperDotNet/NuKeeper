using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Github;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Creators
{
    public class ForkFinderCreator : ICreate<IForkFinder>
    {
        private readonly INuKeeperLogger _logger;
        private readonly ICreate<IGithub> _githubCreator;

        public ForkFinderCreator(INuKeeperLogger logger, ICreate<IGithub> githubCreator)
        {
            _logger = logger;
            _githubCreator = githubCreator;
        }

        public IForkFinder Create(SettingsContainer settings)
        {
            return new ForkFinder(_githubCreator.Create(settings), settings.UserSettings, _logger);
        }
    }
}
