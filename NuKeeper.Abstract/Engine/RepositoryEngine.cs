using System;
using System.Threading.Tasks;
using LibGit2Sharp;
using NuKeeper.Abstract.Configuration;
using NuKeeper.Abstract.Engine;
using NuKeeper.Git;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Engine
{
    public class RepositoryEngine: IRepositoryEngine
    {
        private readonly IRepositoryUpdater _repositoryUpdater;
        private readonly IForkFinder _forkFinder;
        private readonly IFolderFactory _folderFactory;
        private readonly INuKeeperLogger _logger;
        private readonly IRepositoryFilter _repositoryFilter;

        public RepositoryEngine(
            IRepositoryUpdater repositoryUpdater,
            IForkFinder forkFinder,
            IFolderFactory folderFactory,
            INuKeeperLogger logger,
            IRepositoryFilter repositoryFilter)
        {
            _repositoryUpdater = repositoryUpdater;
            _forkFinder = forkFinder;
            _folderFactory = folderFactory;
            _logger = logger;
            _repositoryFilter = repositoryFilter;
        }

        public async Task<int> Run(IRepositorySettings repository,
            UsernamePasswordCredentials gitCreds,
            Identity userIdentity,
            ISettingsContainer settings)
        {
            try
            {
                var repo = await BuildGitRepositorySpec(repository, settings.UserSettings.ForkMode, gitCreds.Username);
                if (repo == null)
                {
                    return 0;
                }

                if (!await _repositoryFilter.ContainsDotNetProjects(repository).ConfigureAwait(false))
                {
                    return 0;
                }

                var tempFolder = _folderFactory.UniqueTemporaryFolder();
                var git = new LibGit2SharpDriver(_logger, tempFolder, gitCreds, userIdentity);

                var updatesDone = await _repositoryUpdater.Run(git, repo, settings);

                tempFolder.TryDelete();
                return updatesDone;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed on repo {repository.RepositoryName}", ex);
                return 0;
            }
        }

        private async Task<RepositoryData> BuildGitRepositorySpec(
            IRepositorySettings repository,
            ForkMode forkMode,
            string userName)
        {
            var pullFork = new ForkData(repository.Uri, repository.Owner, repository.RepositoryName);
            var pushFork = await _forkFinder.FindPushFork(forkMode, userName, pullFork);

            if (pushFork == null)
            {
                _logger.Normal($"No pushable fork found for {repository.Uri}");
                return null;
            }

            return new RepositoryData(pullFork, pushFork);
        }
    }
}
