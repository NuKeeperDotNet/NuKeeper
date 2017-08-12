using System;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Git;
using NuKeeper.Github;
using NuKeeper.Logging;
using NuKeeper.NuGet.Api;
using NuKeeper.NuGet.Process;
using NuKeeper.RepositoryInspection;
using Octokit;

namespace NuKeeper.Engine
{
    public class RepositoryUpdater : IRepositoryUpdater
    {   
        private readonly IPackageUpdatesLookup _packageLookup;
        private readonly IGithub _github;
        private readonly IPackageUpdateSelection _updateSelection;
        private readonly INuKeeperLogger _logger;

        public RepositoryUpdater(
            IGithub github, 
            IPackageUpdatesLookup packageLookup, 
            IPackageUpdateSelection updateSelection, 
            INuKeeperLogger logger)
        {
            _github = github;
            _packageLookup = packageLookup;
            _updateSelection = updateSelection;
            _logger = logger;
        }

        public async Task Run(IGitDriver git, RepositoryModeSettings settings)
        {
            git.Clone(settings.GithubUri);
            var defaultBranch = git.GetCurrentHead();

            // scan for nuget packages
            var repoScanner = new RepositoryScanner();
            var packages = repoScanner.FindAllNuGetPackages(git.WorkingFolder.FullPath)
                .ToList();

            _logger.Log(EngineReport.PackagesFound(packages));

            // look for package updates
            var updates = await _packageLookup.FindUpdatesForPackages(packages);
            _logger.Log(EngineReport.UpdatesFound(updates));

            if (updates.Count == 0)
            {
                _logger.Terse("No potential updates found. Well done. Exiting.");
                return;
            }

            var targetUpdates = _updateSelection.SelectTargets(updates);

            foreach (var updateSet in targetUpdates)
            {
                await UpdatePackageInProjects(git, updateSet, settings, defaultBranch);
            }

            _logger.Info("Done");
        }

        private async Task UpdatePackageInProjects(
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
                var branchName = $"nukeeper-update-{updateSet.PackageId}-to-{updateSet.NewVersion}";
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
