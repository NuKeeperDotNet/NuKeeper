using System;
using System.Threading.Tasks;
using LibGit2Sharp;
using NuKeeper.Configuration;
using NuKeeper.Git;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Engine
{
    public class GithubRepositoryEngine: IGithubRepositoryEngine
    {
        private readonly IRepositoryUpdater _repositoryUpdater;
        private readonly IForkFinder _forkFinder;
        private readonly IFolderFactory _folderFactory;
        private readonly INuKeeperLogger _logger;

        public GithubRepositoryEngine(
            IRepositoryUpdater repositoryUpdater,
            IForkFinder forkFinder,
            IFolderFactory folderFactory,
            INuKeeperLogger logger
            )
        {
            _repositoryUpdater = repositoryUpdater;
            _forkFinder = forkFinder;
            _folderFactory = folderFactory;
            _logger = logger;
        }

        public async Task<int> Run(RepositorySettings repository, UsernamePasswordCredentials gitCreds,
            Identity userIdentity)
        {
            try
            {
                var repo = await BuildGitRepositorySpec(repository, gitCreds.Username);
                if (repo == null)
                {
                    return 0;
                }

                var tempFolder = _folderFactory.UniqueTemporaryFolder();
                var git = new LibGit2SharpDriver(_logger, tempFolder, gitCreds, userIdentity);

                var updatesDone = await _repositoryUpdater.Run(git, repo);

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
            RepositorySettings repository, 
            string userName)
        {
            var pullFork = new ForkData(repository.GithubUri, repository.RepositoryOwner, repository.RepositoryName);
            var pushFork = await _forkFinder.FindPushFork(userName, pullFork);

            if (pushFork == null)
            {
                _logger.Info($"No pushable fork found for {repository.GithubUri}");
                return null;
            }

            return new RepositoryData(pullFork, pushFork);
        }
    }
}
