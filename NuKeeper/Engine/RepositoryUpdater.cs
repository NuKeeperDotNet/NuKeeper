using System;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Git;
using NuKeeper.Github;
using NuKeeper.NuGet.Api;
using NuKeeper.NuGet.Process;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public class RepositoryUpdater
    {
        private readonly IPackageUpdatesLookup _packageLookup;
        private readonly RepositoryModeSettings _settings;
        private readonly string _tempDir;
        private readonly IGitDriver _git;
        private readonly IGithub _github;
        private readonly IPackageUpdateSelection _updateSelection;

        public RepositoryUpdater(IPackageUpdatesLookup packageLookup, 
            IGithub github,
            IPackageUpdateSelection updateSelection,
            RepositoryModeSettings settings)
        {
            _packageLookup = packageLookup;
            _github = github;
            _updateSelection = updateSelection;
            _settings = settings;

            // get some storage space
            _tempDir = TempFiles.MakeUniqueTemporaryPath();
            _git = new GitDriver(_tempDir);
        }

        public async Task Run()
        {
            await GitCloneToTempDir();

            // scan for nuget packages
            var repoScanner = new RepositoryScanner();
            var packages = repoScanner.FindAllNuGetPackages(_tempDir)
                .ToList();

            EngineReport.PackagesFound(packages);

            // look for package updates
            var updates = await _packageLookup.FindUpdatesForPackages(packages);
            EngineReport.UpdatesFound(updates);

            if (updates.Count == 0)
            {
                Console.WriteLine("No potential updates found. Well done. Exiting.");
                return;
            }

            var targetUpdates = _updateSelection.SelectTargets(updates);

            foreach (var updateSet in targetUpdates)
            {
                await UpdatePackageInProjects(updateSet);
            }

            // delete the temp folder
            TempFiles.TryDelete(_tempDir);
            Console.WriteLine("Done");
        }

        private async Task GitCloneToTempDir()
        {
            Console.WriteLine($"Using temp dir: {_tempDir}");
            Console.WriteLine($"Git url: {_settings.GithubUri}");

            await _git.Clone(_settings.GithubUri);

            Console.WriteLine("Git clone complete");
        }

        private async Task UpdatePackageInProjects(PackageUpdateSet updateSet)
        {
            EngineReport.OldVersionsToBeUpdated(updateSet);

            await _git.Checkout("master");

            // branch
            var branchName = $"nukeeper-update-{updateSet.PackageId}-to-{updateSet.NewVersion}";

            await _git.CheckoutNewBranch(branchName);

            Console.WriteLine($"Using branch '{branchName}'");

            await UpdateAllCurrentUsages(updateSet);

            Console.WriteLine("Commiting");

            var commitMessage = CommitReport.MakeCommitMessage(updateSet);
            await _git.Commit(commitMessage);

            Console.WriteLine($"Pushing branch '{branchName}'");
            await _git.Push("origin", branchName);

            await MakeGitHubPullRequest(updateSet, commitMessage, branchName);
            await _git.Checkout("master");
        }

        private static async Task UpdateAllCurrentUsages(PackageUpdateSet updateSet)
        {
            foreach (var current in updateSet.CurrentPackages)
            {
                var updater = current.Path.PackageReferenceType == PackageReferenceType.ProjectFile
                    ? (INuGetUpdater) new NuGetUpdater()
                    : new PackagesConfigUpdater();

                await updater.UpdatePackage(updateSet.NewVersion, current);
            }
        }

        private async Task MakeGitHubPullRequest(PackageUpdateSet updates, string commitMessage, string branchName)
        {
            Console.WriteLine($"Making PR on '{_settings.GithubApiBase} {_settings.RepositoryOwner} {_settings.RepositoryName}'");

            // open github PR
            var pr = new OpenPullRequestRequest
            {
                Data = new PullRequestData
                {
                    Title = commitMessage,
                    Body = CommitReport.MakeCommitDetails(updates),
                    Base = "master",
                    Head = branchName
                },
                RepositoryOwner = _settings.RepositoryOwner,
                RepositoryName = _settings.RepositoryName
            };

            await _github.OpenPullRequest(pr);
        }
    }
}