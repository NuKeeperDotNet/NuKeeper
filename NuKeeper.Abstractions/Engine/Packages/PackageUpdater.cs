using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Git;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update;

namespace NuKeeper.Abstractions.Engine.Packages
{
    public class PackageUpdater : IPackageUpdater
    {
        private readonly IClient _client;
        private readonly INuKeeperLogger _logger;
        private readonly IUpdateRunner _updateRunner;

        public PackageUpdater(
            IClient gitHub,
            IUpdateRunner localUpdater,
            INuKeeperLogger logger)
        {
            _client = gitHub;
            _updateRunner = localUpdater;
            _logger = logger;
        }

        public async Task<int> MakeUpdatePullRequests(
            IGitDriver git,
            IRepositoryData repository,
            IReadOnlyCollection<PackageUpdateSet> updates,
            NuGetSources sources,
            ISettingsContainer settings)
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
            IGitDriver git, IRepositoryData repository,
            NuGetSources sources, ISettingsContainer settings,
            IReadOnlyCollection<PackageUpdateSet> updates)
        {
            _logger.Normal(UpdatesLogger.OldVersionsToBeUpdated(updates));

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

            await _client.CreatePullRequest(repository, title, body, branchName,
                settings.SourceControlServerSettings.Labels);

            git.Checkout(repository.DefaultBranch);
            return updates.Count;
        }
    }
}
