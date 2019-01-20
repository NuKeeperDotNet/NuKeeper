using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Update;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Git;

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
            catch (Exception ex)
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
            var branchWithChanges = BranchNamer.MakeName(updates);
            _logger.Detailed($"Using branch name: '{branchWithChanges}'");
            git.CheckoutNewBranch(branchWithChanges);

            foreach (var updateSet in updates)
            {
                await _updateRunner.Update(updateSet, sources, null);

                var commitMessage = CommitWording.MakeCommitMessage(updateSet);
                git.Commit(commitMessage);
            }

            git.Push(repository.Remote, branchWithChanges);

            var title = CommitWording.MakePullRequestTitle(updates);
            var body = CommitWording.MakeCommitDetails(updates);

            string qualifiedBranch;
            if (repository.Pull.Owner == repository.Push.Owner)
            {
                qualifiedBranch = branchWithChanges;
            }
            else
            {
                qualifiedBranch = repository.Push.Owner + ":" + branchWithChanges;
            }

            var pullRequestRequest = new PullRequestRequest(qualifiedBranch, title, repository.DefaultBranch) { Body = body };

            await _collaborationFactory.CollaborationPlatform.OpenPullRequest(repository.Pull, pullRequestRequest, settings.SourceControlServerSettings.Labels);


            git.Checkout(repository.DefaultBranch);
            return updates.Count;
        }
    }
}
