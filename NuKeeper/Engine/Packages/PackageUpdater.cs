using System;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Engine.FilesUpdate;
using NuKeeper.Git;
using NuKeeper.Github;
using NuKeeper.Logging;
using NuKeeper.NuGet.Process;
using NuKeeper.RepositoryInspection;
using Octokit;

namespace NuKeeper.Engine.Packages
{
    public class PackageUpdater : IPackageUpdater
    {
        private readonly IGithub _github;
        private readonly IConfigFilesUpdater _configFilesUpdater;
        private readonly INuKeeperLogger _logger;
        private readonly UserSettings _settings;

        public PackageUpdater(
            IGithub github,
            IConfigFilesUpdater configFilesUpdater,
            INuKeeperLogger logger,
            UserSettings settings)
        {
            _github = github;
            _configFilesUpdater = configFilesUpdater;
            _logger = logger;
            _settings = settings;
        }

        public async Task UpdatePackageInProjects(
            IGitDriver git,
            PackageUpdateSet updateSet,
            RepositoryData repository)
        {
            try
            {
                _logger.Terse(UpdatesLogger.OldVersionsToBeUpdated(updateSet));

                git.Checkout(repository.DefaultBranch);

                // branch
                var branchName = BranchNamer.MakeName(updateSet);
                _logger.Verbose($"Using branch name: '{branchName}'");
                git.CheckoutNewBranch(branchName);

                await UpdateAllCurrentUsages(updateSet);
                UpdateConfigFiles(git, updateSet);

                var commitMessage = CommitWording.MakeCommitMessage(updateSet);
                git.Commit(commitMessage);

                git.Push("nukeeper_push", branchName);

                var prTitle = CommitWording.MakePullRequestTitle(updateSet);
                await MakeGitHubPullRequest(updateSet, repository, prTitle, branchName);

                git.Checkout(repository.DefaultBranch);
            }
            catch (Exception ex)
            {
                _logger.Error("Update failed", ex);
            }
        }

        private async Task UpdateAllCurrentUsages(PackageUpdateSet updateSet)
        {
            foreach (var current in updateSet.CurrentPackages)
            {
                var restoreCommand = GetRestoreCommand(current.Path.PackageReferenceType);
                if (restoreCommand != null)
                {
                    await restoreCommand.Invoke(current.Path.Info);
                }

                var updateCommand = GetUpdateCommand(current.Path.PackageReferenceType);
                await updateCommand.Invoke(updateSet.SelectedVersion, updateSet.Selected.Source, current);
            }
        }

        private void UpdateConfigFiles(IGitDriver git, PackageUpdateSet updateSet)
        {
            var targetVersion = updateSet.Selected.Identity;
            foreach (var currentVersion in updateSet.CurrentPackages)
            {
                _configFilesUpdater.Update(git.WorkingFolder, currentVersion.Identity, targetVersion);
            }
        }
        private IFileRestoreCommand GetRestoreCommand(PackageReferenceType packageReferenceType)
        {
            if (packageReferenceType != PackageReferenceType.ProjectFile)
            {
                return new NuGetFileRestoreCommand(_logger, _settings);
            }

            return null;
        }

        private IUpdatePackageCommand GetUpdateCommand(PackageReferenceType packageReferenceType)
        {
            if (packageReferenceType != PackageReferenceType.PackagesConfig)
            {
                return new DotNetUpdatePackageCommand(_logger, _settings);
            }

            return new NuGetUpdatePackageCommand(_logger, _settings);
        }

        private async Task MakeGitHubPullRequest(
            PackageUpdateSet updates,
            RepositoryData repository,
            string title, string branchWithChanges)
        {
            string qualifiedBranch;
            if (repository.Pull.Owner == repository.Push.Owner)
            {
                qualifiedBranch = branchWithChanges;
            }
            else
            {
                qualifiedBranch = repository.Push.Owner + ":" + branchWithChanges;
            }

            var pr = new NewPullRequest(title, qualifiedBranch, repository.DefaultBranch)
            {
                Body = CommitWording.MakeCommitDetails(updates)
            };

            await _github.OpenPullRequest(repository.Pull, pr);
        }
    }
}
