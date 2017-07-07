using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Git;
using NuKeeper.Github;
using NuKeeper.Nuget.Api;
using NuKeeper.Nuget.Process;
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
            Console.WriteLine($"Using temp dir: {_tempDir}");

            // clone the repo
            Console.WriteLine($"Git url: {_settings.GithubUri}");
            await _git.Clone(_settings.GithubUri);

            Console.WriteLine("Git clone complete");

            // scan for nuget packages
            var repoScanner = new RepositoryScanner();
            var packages = repoScanner.FindAllNugetPackages(_tempDir)
                .ToList();

            var packageNames = string.Join(",", packages.Take(10).Select(p => p.Id));
            Console.WriteLine($"Found {packages.Count} packages: {packageNames}");

            // look for package updates
            var updates = await _packageLookup.FindUpdatesForPackages(packages);
            var updateDetails = updates.Take(10)
                .Select(p => $"{p.PackageId} from {p.OldVersion} to {p.NewVersion}");

            Console.WriteLine($"Found {updates.Count} updates: {string.Join(",", updateDetails)}");

            if (updates.Count == 0)
            {
                Console.WriteLine("No potential updates found. Well done. Exiting.");
                return;
            }

            // All packages that need update
            var updatesByPackage = updates.GroupBy(p => p.PackageId);

            // limit!!!
            updatesByPackage = updatesByPackage.Take(2);

            foreach (var packageUpdates in updatesByPackage)
            {
                await UpdatePackageInProjects(packageUpdates.Key, packageUpdates.ToList());
            }

            // delete the temp folder
            TempFiles.TryDelete(_tempDir);
            Console.WriteLine("Done");
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
            var branchName = $"nukeeper-update-{packageId}-from-{oldVersionsString}-to-{firstUpdate.NewVersion}";

            await _git.CheckoutNewBranch(branchName);

            Console.WriteLine($"Using branch '{branchName}'");

            var nugetUpdater = new NugetUpdater();

            foreach (var update in updates)
            {
                await nugetUpdater.UpdatePackage(update);
            }

            Console.WriteLine("Commiting");

            var reporter = new CommitReport(_tempDir);
            var commitMessage = reporter.MakeCommitMessage(updates);
            await _git.Commit(commitMessage);

            Console.WriteLine($"Pushing branch '{branchName}'");
            await _git.Push("origin", branchName);

            Console.WriteLine($"Making PR on '{_settings.GithubApiBase} {_settings.RepositoryOwner} {_settings.RepositoryName}'");

            // open github PR
            var pr = new OpenPullRequestRequest
            {
                Data = new PullRequestData
                {
                    Title = commitMessage,
                    Body = reporter.MakeCommitDetails(updates),
                    Base = "master",
                    Head = branchName
                },
                RepositoryOwner = _settings.RepositoryOwner,
                RepositoryName = _settings.RepositoryName
            };

            await _github.OpenPullRequest(pr);
            await _git.Checkout("master");
        }
    }
}