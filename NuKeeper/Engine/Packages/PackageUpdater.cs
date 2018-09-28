using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Git;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update;

namespace NuKeeper.Engine.Packages
{
    public class PackageUpdater : IPackageUpdater
    {
        private readonly IGitHub _gitHub;
        private readonly INuKeeperLogger _logger;
        private readonly IUpdateRunner _updateRunner;

        public PackageUpdater(
            IGitHub gitHub,
            IUpdateRunner localUpdater,
            INuKeeperLogger logger)
        {
            _gitHub = gitHub;
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
                _logger.Minimal(UpdatesLogger.OldVersionsToBeUpdated(updates));

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
                _logger.Error("Update failed", ex);
            }

            return totalCount;
        }

        private async Task<int> MakeUpdatePullRequests(
            IGitDriver git, RepositoryData repository,
            NuGetSources sources, SettingsContainer settings,
            IReadOnlyCollection<PackageUpdateSet> updates)
        {
            git.Checkout(repository.DefaultBranch);

            // branch
            var branchName = BranchNamer.MakeName(updates);
            _logger.Detailed($"Using branch name: '{branchName}'");
            git.CheckoutNewBranch(branchName);

            foreach (var updateSet in updates)
            {
                await _updateRunner.Update(updateSet, sources);

                var commitMessage = CommitWording.MakeCommitMessage(updateSet);
                git.Commit(commitMessage);
            }

            git.Push("nukeeper_push", branchName);

            var title = CommitWording.MakePullRequestTitle(updates);
            var body = CommitWording.MakeCommitDetails(updates);
            await _gitHub.CreatePullRequest(repository, title, body, branchName,
                settings.SourceControlServerSettings.Labels);

            git.Checkout(repository.DefaultBranch);
            return updates.Count;
        }
    }
}
