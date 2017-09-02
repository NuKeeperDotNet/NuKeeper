using System;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Git;
using NuKeeper.Github;
using NuKeeper.Logging;
using NuKeeper.NuGet.Process;
using NuKeeper.RepositoryInspection;
using Octokit;

namespace NuKeeper.Engine
{
    public class PackageUpdater : IPackageUpdater
    {
        private readonly IGithub _github;
        private readonly INuKeeperLogger _logger;
        private readonly Settings _settings;

        public PackageUpdater(
            IGithub github,
            INuKeeperLogger logger,
            Settings settings)
        {
            _github = github;
            _logger = logger;
            _settings = settings;
        }

        public async Task UpdatePackageInProjects(
            IGitDriver git,
            PackageUpdateSet updateSet,
            RepositoryModeSettings settings,
            string defaultBranch)
        {
            try
            {
                _logger.Terse(EngineReport.OldVersionsToBeUpdated(updateSet));

                git.Checkout(defaultBranch);

                // branch
                var branchName = BranchNamer.MakeName(updateSet);
                _logger.Verbose($"Using branch name: '{branchName}'");
                git.CheckoutNewBranch(branchName);

                await UpdateAllCurrentUsages(updateSet);

                var commitMessage = CommitReport.MakeCommitMessage(updateSet);
                git.Commit(commitMessage);

                git.Push("origin", branchName);

                var prTitle = CommitReport.MakePullRequestTitle(updateSet);
                await MakeGitHubPullRequest(updateSet, settings, prTitle, branchName, defaultBranch);

                git.Checkout(defaultBranch);
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
                    await restoreCommand.Invoke(current);
                }

                var updateCommand = GetUpdateCommand(current.Path.PackageReferenceType);
                await updateCommand.Invoke(updateSet.NewVersion, updateSet.PackageSource, current);
            }
        }

        private INuGetProjectRestoreCommand GetRestoreCommand(PackageReferenceType packageReferenceType)
        {
            if (packageReferenceType == PackageReferenceType.PackagesConfig)
            {
                return new NuGetProjectRestoreCommand(_logger, _settings);
            }

            return null;
        }

        private IUpdatePackageCommand GetUpdateCommand(PackageReferenceType packageReferenceType)
        {
            if (packageReferenceType == PackageReferenceType.ProjectFile)
            {
                return new DotNetUpdatePackageCommand(_logger, _settings);
            }

            return new NuGetUpdatePackageCommand(_logger, _settings);
        }

        private async Task MakeGitHubPullRequest(
            PackageUpdateSet updates,
            RepositoryModeSettings settings,
            string title, string headBranch, string baseBranch)
        {
            var pr = new NewPullRequest(title, headBranch, baseBranch)
            {
                Body = CommitReport.MakeCommitDetails(updates)
            };

            await _github.OpenPullRequest(settings.RepositoryOwner, settings.RepositoryName, pr);
        }

    }
}
