using System;
using System.Collections.Generic;
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

        public RepositoryUpdater(IPackageUpdatesLookup packageLookup, IGithub github, RepositoryModeSettings settings)
        {
            _packageLookup = packageLookup;
            _settings = settings;

            // get some storage space
            _tempDir = TempFiles.MakeUniqueTemporaryPath();
            _git = new GitDriver(_tempDir);
            _github = github;
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

            var updatesByPackage = GroupUpdatesByPackageId(updates);

            foreach (var packageUpdates in updatesByPackage)
            {
                await UpdatePackageInProjects(packageUpdates.Key, packageUpdates.ToList());
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

        private static IEnumerable<IGrouping<string, PackageUpdate>> GroupUpdatesByPackageId(List<PackageUpdate> updates)
        {
            // All packages that need update
            var updatesByPackage = updates.GroupBy(p => p.PackageId);

            // todo make this number config
            return updatesByPackage.Take(2);
        }

        private async Task UpdatePackageInProjects(string packageId, List<PackageUpdate> updates)
        {
            var oldVersions = updates
                .Select(u => u.OldVersion.ToString())
                .Distinct();

            var oldVersionsString = string.Join(",", oldVersions);

            var firstUpdate = updates.First();

            Console.WriteLine($"Updating '{packageId}' from {oldVersionsString} to {firstUpdate.NewVersion} in {updates.Count} projects");

            await _git.Checkout("master");

            // branch
            var branchName = $"nukeeper-update-{packageId}-to-{firstUpdate.NewVersion}";

            await _git.CheckoutNewBranch(branchName);

            Console.WriteLine($"Using branch '{branchName}'");

            foreach (var update in updates)
            {
                var updater = update.CurrentPackage.PackageReferenceType == PackageReferenceType.ProjectFile
                    ? (INuGetUpdater) new NuGetUpdater()
                    : new PackagesConfigUpdater();

                await updater.UpdatePackage(update);
            }

            Console.WriteLine("Commiting");

            var commitMessage = CommitReport.MakeCommitMessage(updates);
            await _git.Commit(commitMessage);

            Console.WriteLine($"Pushing branch '{branchName}'");
            await _git.Push("origin", branchName);

            await MakeGitHubPullRequest(updates, commitMessage, branchName);
            await _git.Checkout("master");
        }

        private async Task MakeGitHubPullRequest(List<PackageUpdate> updates, string commitMessage, string branchName)
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