using NuKeeper.Configuration;
using NuKeeper.Engine.Packages;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Logging;
using NuKeeper.Update;

namespace NuKeeper.Creators
{
    public class PackageUpdaterCreator : ICreate<IPackageUpdater>
    {
        private readonly INuKeeperLogger _logger;
        private readonly ICreate<IGitHub> _githubCreator;
        private readonly IUpdateRunner _updateRunner;

        public PackageUpdaterCreator(INuKeeperLogger logger, ICreate<IGitHub> githubCreator, IUpdateRunner updateRunner)
        {
            _logger = logger;
            _githubCreator = githubCreator;
            _updateRunner = updateRunner;
        }

        public IPackageUpdater Create(SettingsContainer settings)
        {
            return new PackageUpdater(_githubCreator.Create(settings), _updateRunner, _logger);
        }
    }
}
