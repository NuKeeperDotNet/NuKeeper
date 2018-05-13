using System;
using System.Threading.Tasks;
using NuKeeper.Git;
using NuKeeper.Github;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using Octokit;

namespace NuKeeper.Engine.Packages
{
    public class PackageUpdater : IPackageUpdater
    {
        private readonly IGithub _github;
        private readonly INuKeeperLogger _logger;
        private readonly IUpdateRunner _updateRunner;

        public PackageUpdater(
            IGithub github,
            IUpdateRunner localUpdater,
            INuKeeperLogger logger)
        {
            _github = github;
            _updateRunner = localUpdater;
            _logger = logger;
        }

        public async Task MakeUpdatePullRequest(
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

                await _updateRunner.Update(updateSet);

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
