using System;
using System.Threading.Tasks;
using LibGit2Sharp;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.DTOs;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Git;
using NuKeeper.Inspection.Files;

namespace NuKeeper.Engine
{
    public class GitRepositoryEngine: IGitRepositoryEngine
    {
        private readonly IRepositoryUpdater _repositoryUpdater;
        private readonly ICollaborationFactory _collaborationFactory;
        private readonly IFolderFactory _folderFactory;
        private readonly INuKeeperLogger _logger;
        private readonly IRepositoryFilter _repositoryFilter;

        public GitRepositoryEngine(
            IRepositoryUpdater repositoryUpdater,
            ICollaborationFactory collaborationFactory,
            IFolderFactory folderFactory,
            INuKeeperLogger logger,
            IRepositoryFilter repositoryFilter)
        {
            _repositoryUpdater = repositoryUpdater;
            _collaborationFactory = collaborationFactory;
            _folderFactory = folderFactory;
            _logger = logger;
            _repositoryFilter = repositoryFilter;
        }

        public async Task<int> Run(RepositorySettings repository,
            UsernamePasswordCredentials gitCreds,
            Identity userIdentity,
            SettingsContainer settings)
        {
            try
            {
                var repo = await BuildGitRepositorySpec(repository, settings.UserSettings.ForkMode, gitCreds.Username);
                if (repo == null)
                {
                    return 0;
                }

                if (!await _repositoryFilter.ContainsDotNetProjects(repository))
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
            RepositorySettings repository,
            ForkMode forkMode,
            string userName)
        {
            var pullFork = new ForkData(repository.RepositoryUri, repository.RepositoryOwner, repository.RepositoryName);
            var pushFork = await _collaborationFactory.ForkFinder.FindPushFork(forkMode, userName, pullFork);

            if (pushFork == null)
            {
                _logger.Normal($"No pushable fork found for {repository.RepositoryUri}");
                return null;
            }

            return new RepositoryData(pullFork, pushFork);
        }
    }
}
