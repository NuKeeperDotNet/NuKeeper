using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Creators
{
    public class GithubRepositoryEngineCreator : ICreate<IGithubRepositoryEngine>
    {
        private readonly INuKeeperLogger _logger;
        private readonly IFolderFactory _folderFactory;
        private readonly ICreate<IForkFinder> _forkFinderCreator;
        private readonly ICreate<IRepositoryUpdater> _repositoryUpdaterCreator;
        private readonly ICreate<IRepositoryFilter> _repositoryFilterCreator;

        public GithubRepositoryEngineCreator(INuKeeperLogger logger,
            IFolderFactory folderFactory, ICreate<IForkFinder> forkFinderCreator, ICreate<IRepositoryUpdater> repositoryUpdaterCreator, ICreate<IRepositoryFilter> repositoryFilterCreator)
        {
            _logger = logger;
            _folderFactory = folderFactory;
            _forkFinderCreator = forkFinderCreator;
            _repositoryUpdaterCreator = repositoryUpdaterCreator;
            _repositoryFilterCreator = repositoryFilterCreator;
        }

        public IGithubRepositoryEngine Create(SettingsContainer settings)
        {
            return new GithubRepositoryEngine(_repositoryUpdaterCreator.Create(settings), _forkFinderCreator.Create(settings), _folderFactory, _logger,
                _repositoryFilterCreator.Create(settings));
        }
    }
}
