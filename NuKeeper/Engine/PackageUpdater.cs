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

        public PackageUpdater(
            IGithub github,
            INuKeeperLogger logger)
        {
            _github = github;
            _logger = logger;
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
                var branchName = MakeBranchName(git, updateSet);
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

        private string MakeBranchName(IGitDriver git, PackageUpdateSet updateSet)
        {
            var branchName = $"nukeeper-update-{updateSet.PackageId}-to-{updateSet.NewVersion}";
            _logger.Verbose($"Using branch name: '{branchName}'");

            var qualifiedBranchName = "origin/" + branchName;

            if (git.BranchExists(qualifiedBranchName))
            {
                throw new Exception($"A Git branch named '{qualifiedBranchName}' already exists");
            }

            return branchName;
        }

        private async Task UpdateAllCurrentUsages(PackageUpdateSet updateSet)
        {
            foreach (var current in updateSet.CurrentPackages)
            {
                var updateCommand = GetUpdateCommand(current.Path.PackageReferenceType);
                await updateCommand.Invoke(updateSet.NewVersion, current);
            }
        }

        private IUpdatePackageCommand GetUpdateCommand(PackageReferenceType packageReferenceType)
        {
            if (packageReferenceType == PackageReferenceType.ProjectFile)
            {
                return new DotNetUpdatePackageCommand(_logger);
            }

            return new NuGetUpdatePackageCommand(_logger);
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
