
using System;
using System.Threading.Tasks;
using LibGit2Sharp;
using NuKeeper.Configuration;
using NuKeeper.Files;
using NuKeeper.Git;
using NuKeeper.Logging;

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

        public async Task Run(RepositorySettings repository, UsernamePasswordCredentials gitCreds)
        {
            try
            {
                var tempFolder = _folderFactory.UniqueTemporaryFolder();
                var git = new LibGit2SharpDriver(_logger, tempFolder, gitCreds);
                var repo = await BuildGitRepositorySpec(repository, gitCreds.Username);

                await _repositoryUpdater.Run(git, repo);

                tempFolder.TryDelete();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed on repo {repository.RepositoryName}", ex);
            }
        }

        private async Task<RepositoryData> BuildGitRepositorySpec(
            RepositorySettings repository, 
            string userName)
        {
            var pullFork = new ForkData(repository.GithubUri, repository.RepositoryOwner, repository.RepositoryName);
            var pushFork = await _forkFinder.PushFork(userName, repository.RepositoryName, pullFork);

            return new RepositoryData(pullFork, pushFork);
        }
    }
}
