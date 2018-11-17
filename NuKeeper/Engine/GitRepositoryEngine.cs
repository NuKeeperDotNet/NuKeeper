using System;
using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.DTOs;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Git;
using NuKeeper.Inspection.Files;

namespace NuKeeper.Engine
{
    public class GitRepositoryEngine : IGitRepositoryEngine
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
                var repo = await BuildGitRepositorySpec(repository, gitCreds.Username);
                if (repo == null)
                {
                    return 0;
                }

                if (!repository.IsLocalRepo) // The updaters will do the check for the local files, and they know what file types they can handle.
                {
                    if (!await _repositoryFilter.ContainsDotNetProjects(repository))
                    {
                        return 0;
                    }
                }

                IFolder folder = null;
                if (repository.IsLocalRepo)
                {
                    folder = new Folder(_logger, new DirectoryInfo(repository.RemoteInfo.LocalRepositoryUri.AbsolutePath));
                    settings.WorkingFolder = new Folder(_logger, new DirectoryInfo(repository.RemoteInfo.WorkingFolder.AbsolutePath));
                    repo.DefaultBranch = repository.RemoteInfo.BranchName;
                    repo.Remote = repository.RemoteInfo.RemoteName;
                }
                else
                {
                    folder = _folderFactory.UniqueTemporaryFolder();
                    settings.WorkingFolder = folder;
                }

                var git = new LibGit2SharpDriver(_logger, folder, gitCreds, userIdentity);

                var updatesDone = await _repositoryUpdater.Run(git, repo, settings);

                if (!repository.IsLocalRepo)
                    folder.TryDelete();

                return updatesDone;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed on repo {repository.RepositoryName}", ex);
                return 1;
            }
        }

        private async Task<RepositoryData> BuildGitRepositorySpec(
            RepositorySettings repository,
            string userName)
        {
            var pullFork = new ForkData(repository.RepositoryUri, repository.RepositoryOwner, repository.RepositoryName);
            var pushFork = await _collaborationFactory.ForkFinder.FindPushFork(userName, pullFork);

            if (pushFork == null)
            {
                _logger.Normal($"No pushable fork found for {repository.RepositoryUri}");
                return null;
            }

            return new RepositoryData(pullFork, pushFork);
        }
    }
}
