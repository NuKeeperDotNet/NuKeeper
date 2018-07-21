using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Engine.Packages;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update.Process;

namespace NuKeeper.Creators
{
    public class RepositoryUpdaterCreator : ICreate<IRepositoryUpdater>
    {
        private readonly INuKeeperLogger _logger;
        private readonly INuGetSourcesReader _sourcesReader;
        private readonly IUpdateFinder _updateFinder;
        private readonly ICreate<IPackageUpdater> _packageUpdaterCreator;
        private readonly SolutionsRestore _solutionRestore;
        private readonly IAvailableUpdatesReporter _reporter;
        private readonly ICreate<IPackageUpdateSelection> _packageUpdateSelectionCreator;

        public RepositoryUpdaterCreator(INuKeeperLogger logger, INuGetSourcesReader sourcesReader,
            IUpdateFinder updateFinder, ICreate<IPackageUpdater> packageUpdaterCreator,
            SolutionsRestore solutionRestore, IAvailableUpdatesReporter reporter, ICreate<IPackageUpdateSelection> packageUpdateSelectionCreator)
        {
            _logger = logger;
            _sourcesReader = sourcesReader;
            _updateFinder = updateFinder;
            _packageUpdaterCreator = packageUpdaterCreator;
            _solutionRestore = solutionRestore;
            _reporter = reporter;
            _packageUpdateSelectionCreator = packageUpdateSelectionCreator;
        }

        public IRepositoryUpdater Create(SettingsContainer settings)
        {
            return new RepositoryUpdater(_sourcesReader, _updateFinder, _packageUpdateSelectionCreator.Create(settings),
                _packageUpdaterCreator.Create(settings), _logger,
                _solutionRestore, _reporter, settings.UserSettings);
        }
    }
}
