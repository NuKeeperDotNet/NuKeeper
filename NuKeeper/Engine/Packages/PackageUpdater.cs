using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Update;

namespace NuKeeper.Engine.Packages
{
    public class PackageUpdater : IPackageUpdater
    {
        private readonly ICollaborationFactory _collaborationFactory;
        private readonly INuKeeperLogger _logger;
        private readonly IUpdateRunner _updateRunner;

        public PackageUpdater(
            ICollaborationFactory collaborationFactory,
            IUpdateRunner localUpdater,
            INuKeeperLogger logger)
        {
            _collaborationFactory = collaborationFactory;
            _updateRunner = localUpdater;
            _logger = logger;
        }

        public async Task<int> MakeUpdatePullRequests(
            IGitDriver git,
            RepositoryData repository,
            IReadOnlyCollection<PackageUpdateSet> updates,
            NuGetSources sources,
            SettingsContainer settings)
        {
            int totalCount = 0;
            try
            {
                var groups = UpdateConsolidator.Consolidate(updates,
                    settings.UserSettings.ConsolidateUpdatesInSinglePullRequest);

                foreach (var updateSets in groups)
                {
                    var updatesMade = await MakeUpdatePullRequests(
                        git, repository,
                        sources, settings, updateSets);

                    totalCount += updatesMade;
                }
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _logger.Error("Updates failed", ex);
            }

            return totalCount;
        }

        private async Task<int> MakeUpdatePullRequests(
            IGitDriver git, RepositoryData repository,
            NuGetSources sources, SettingsContainer settings,
            IReadOnlyCollection<PackageUpdateSet> updates)
        {
            _logger.Normal(UpdatesLogger.OldVersionsToBeUpdated(updates));

            git.Checkout(repository.DefaultBranch);

            // branch
            var branchWithChanges = BranchNamer.MakeName(updates, settings.BranchSettings.BranchNamePrefix);
            _logger.Detailed($"Using branch name: '{branchWithChanges}'");
            git.CheckoutNewBranch(branchWithChanges);

            foreach (var updateSet in updates)
            {
                await _updateRunner.Update(updateSet, sources);

                var commitMessage = _collaborationFactory.CommitWorder.MakeCommitMessage(updateSet);
                git.Commit(commitMessage);
            }

            git.Push(repository.Remote, branchWithChanges);

            var title = _collaborationFactory.CommitWorder.MakePullRequestTitle(updates);
            var body = _collaborationFactory.CommitWorder.MakeCommitDetails(updates);

            string qualifiedBranch;
            if (repository.Pull.Owner == repository.Push.Owner) //check if we are on a fork, if so qualify the branch name
            {
                qualifiedBranch = branchWithChanges;
            }
            else
            {
                qualifiedBranch = repository.Push.Owner + ":" + branchWithChanges;
            }

            var pullRequestRequest = new PullRequestRequest(qualifiedBranch, title, repository.DefaultBranch, settings.BranchSettings.DeleteBranchAfterMerge) { Body = body };

            await _collaborationFactory.CollaborationPlatform.OpenPullRequest(repository.Pull, pullRequestRequest, settings.SourceControlServerSettings.Labels);


            git.Checkout(repository.DefaultBranch);
            return updates.Count;
        }
    }
}
