using System;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Files;
using NuKeeper.Git;
using NuKeeper.Github;
using NuKeeper.Logging;
using NuKeeper.NuGet.Api;
using NuKeeper.NuGet.Process;
using NuKeeper.RepositoryInspection;
using Octokit;

namespace NuKeeper.Engine
{
    public class RepositoryUpdater
    {   
        private readonly IPackageUpdatesLookup _packageLookup;
        private readonly RepositoryModeSettings _settings;
        private readonly IFolder _tempFolder;
        private readonly IGithub _github;
        private readonly IGitDriver _git;
        private readonly IPackageUpdateSelection _updateSelection;
        private readonly INuKeeperLogger _logger;

        public RepositoryUpdater(
            IGithub github, 
            IGitDriver git, 
            IPackageUpdatesLookup packageLookup, 
            IPackageUpdateSelection updateSelection, 
            IFolder tempFolder, 
            INuKeeperLogger logger, 
            RepositoryModeSettings settings)
        {
            _packageLookup = packageLookup;
            _github = github;
            _git = git;
            _logger = logger;

            _tempFolder = tempFolder;
            _updateSelection = updateSelection;
            _settings = settings;
        }

        public async Task Run()
        {
            _git.Clone(_settings.GithubUri);
            var defaultBranch = _git.GetCurrentHead();

            // scan for nuget packages
            var repoScanner = new RepositoryScanner();
            var packages = repoScanner.FindAllNuGetPackages(_tempFolder.FullPath)
                .ToList();

            _logger.Terse(EngineReport.PackagesFoundSummary(packages));
            _logger.Info(EngineReport.PackagesFoundDetails(packages));

            // look for package updates
            var updates = await _packageLookup.FindUpdatesForPackages(packages);
            _logger.Terse(EngineReport.UpdatesFoundSummary(updates));
            _logger.Info(EngineReport.UpdatesFoundDetails(updates));

            if (updates.Count == 0)
            {
                _logger.Terse("No potential updates found. Well done. Exiting.");
                return;
            }

            var targetUpdates = _updateSelection.SelectTargets(updates);

            foreach (var updateSet in targetUpdates)
            {
                await UpdatePackageInProjects(updateSet, defaultBranch);
            }

            // delete the temp folder
            _tempFolder.TryDelete();
            _logger.Info("Done");
        }

        private async Task UpdatePackageInProjects(PackageUpdateSet updateSet, string defaultBranch)
        {
            try
            {
                _logger.Terse(EngineReport.OldVersionsToBeUpdated(updateSet));

                _git.Checkout(defaultBranch);

                // branch
                var branchName = $"nukeeper-update-{updateSet.PackageId}-to-{updateSet.NewVersion}";
                 _git.CheckoutNewBranch(branchName);

                await UpdateAllCurrentUsages(updateSet);

                var commitMessage = CommitReport.MakeCommitMessage(updateSet);
                _git.Commit(commitMessage);

                _git.Push("origin", branchName);

                var prTitle = CommitReport.MakePullRequestTitle(updateSet);
                await MakeGitHubPullRequest(updateSet, prTitle, branchName, defaultBranch);

                _git.Checkout(defaultBranch);
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

        private async Task MakeGitHubPullRequest(PackageUpdateSet updates, string title, string headBranch, string baseBranch)
        {
            var pr = new NewPullRequest(title, headBranch, baseBranch)
                {
                    Body = CommitReport.MakeCommitDetails(updates)
                };

            await _github.OpenPullRequest(_settings.RepositoryOwner, _settings.RepositoryName, pr);
        }
    }
}
