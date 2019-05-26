using System;
using System.IO;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Git;
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
            GitUsernamePasswordCredentials credentials,
            SettingsContainer settings, User user)
        {
            try
            {
                var repositoryData = await BuildGitRepositorySpec(repository, credentials.Username);
                if (repositoryData == null)
                {
                    return 0;
                }

                // should perform the remote check for "is this a .NET repo"
                // (and also not a github fork)
                // only when we have multiple remote repos
                // otherwise it's ok to work locally, and check there
                if (!(settings.SourceControlServerSettings.Scope == ServerScope.Repository || repository.IsLocalRepo))
                {
                    var remoteRepoContainsDotNet = await _repositoryFilter.ContainsDotNetProjects(repository);
                    if (!remoteRepoContainsDotNet)
                    {
                        return 0;
                    }
                }

                IFolder folder;
                if (repository.IsLocalRepo)
                {
                    folder = new Folder(_logger, new DirectoryInfo(repository.RemoteInfo.LocalRepositoryUri.AbsolutePath));
                    settings.WorkingFolder = new Folder(_logger, new DirectoryInfo(repository.RemoteInfo.WorkingFolder.AbsolutePath));

                    if (!repositoryData.IsFork) //check if we are on a fork. If not on a fork we set the remote to the locally found remote
                    {
                        repositoryData.Remote = repository.RemoteInfo.RemoteName;
                    }
                }
                else
                {
                    folder = _folderFactory.UniqueTemporaryFolder();
                    settings.WorkingFolder = folder;
                }

                if (!string.IsNullOrEmpty(repository.RemoteInfo?.BranchName))
                {
                    repositoryData.DefaultBranch = repository.RemoteInfo.BranchName;
                }

                repositoryData.IsLocalRepo = repository.IsLocalRepo;

                var git = new LibGit2SharpDriver(_logger, folder, credentials, user);

                var updatesDone = await _repositoryUpdater.Run(git, repositoryData, settings);

                if (!repository.IsLocalRepo)
                {
                    folder.TryDelete();
                }

                return updatesDone;
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
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
