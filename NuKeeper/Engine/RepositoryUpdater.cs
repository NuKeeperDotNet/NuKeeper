using System;
using System.IO;
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
    public class RepositoryUpdater
    {   
        private readonly IPackageUpdatesLookup _packageLookup;
        private readonly RepositoryModeSettings _settings;
        private readonly string _tempDir;
        private readonly IGithub _github;
        private readonly IGitDriver _git;
        private readonly IPackageUpdateSelection _updateSelection;
        private readonly INuKeeperLogger _logger;

        public RepositoryUpdater(IPackageUpdatesLookup packageLookup, 
            IGithub github,
            IGitDriver git,
            INuKeeperLogger logger,
            string tempDir,
            IPackageUpdateSelection updateSelection,
            RepositoryModeSettings settings)
        {
            _packageLookup = packageLookup;
            _github = github;
            _git = git;
            _logger = logger;

            _tempDir = tempDir;
            _updateSelection = updateSelection;
            _settings = settings;
        }

        public async Task Run()
        {
            GitCloneToTempDir();
            var defaultBranch = _git.GetCurrentHead();

            // scan for nuget packages
            var repoScanner = new RepositoryScanner();
            var packages = repoScanner.FindAllNuGetPackages(_tempDir)
                .ToList();

            _logger.Info(EngineReport.PackagesFound(packages));

            // look for package updates
            var updates = await _packageLookup.FindUpdatesForPackages(packages);
            _logger.Info(EngineReport.UpdatesFound(updates));

            if (updates.Count == 0)
            {
                _logger.Info("No potential updates found. Well done. Exiting.");
                return;
            }

            var targetUpdates = _updateSelection.SelectTargets(updates);

            foreach (var updateSet in targetUpdates)
            {
                await UpdatePackageInProjects(updateSet, defaultBranch);
            }

            // delete the temp folder
            TempFiles.TryDelete(new DirectoryInfo(_tempDir), _logger);
            _logger.Info("Done");
        }

        private void GitCloneToTempDir()
        {
            _logger.Verbose($"Using temp dir: {_tempDir}");
            _logger.Verbose($"Git url: {_settings.GithubUri}");

            _git.Clone(_settings.GithubUri);

            _logger.Verbose("Git clone complete");
        }

        private async Task UpdatePackageInProjects(PackageUpdateSet updateSet, string defaultBranch)
        {
            try
            {
                _logger.Info(EngineReport.OldVersionsToBeUpdated(updateSet));

                _git.Checkout(defaultBranch);

                // branch
                var branchName = $"nukeeper-update-{updateSet.PackageId}-to-{updateSet.NewVersion}";

                 _git.CheckoutNewBranch(branchName);

                _logger.Info($"Using branch '{branchName}'");

                await UpdateAllCurrentUsages(updateSet);

                _logger.Verbose("Commiting");

                var commitMessage = CommitReport.MakeCommitMessage(updateSet);
                _git.Commit(commitMessage);

                _logger.Verbose($"Pushing branch '{branchName}'");
                _git.Push("origin", branchName);

                var prTitle = CommitReport.MakePullRequestTitle(updateSet);
                _logger.Verbose($"Making pull request '{prTitle}'");
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
