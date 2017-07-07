using System;
using System.Collections.Generic;
using System.IO;
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

            foreach (var packageUpdates in updatesByPackage)
            {
                await UpdatePackageInProjects(packageUpdates.Key, packageUpdates.ToList());
            }

            // delete the temp folder
            TryDelete(_tempDir);
            Console.WriteLine("Done");
        }

        private async Task UpdatePackageInProjects(string packageId, List<PackageUpdate> updates)
        {
            var oldVersionsString = OldVersionsString(updates);
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
            var commitMessage = MakeCommitMessage(updates, false);
            await _git.Commit(commitMessage);

            Console.WriteLine($"Pushing branch '{branchName}'");
            await _git.Push("origin", branchName);

            Console.WriteLine($"Making PR on '{_settings.GithubBaseUri} {_settings.RepositoryOwner} {_settings.RepositoryName}'");

            // open github PR
            var pr = new OpenPullRequestRequest
            {
                Data = new PullRequestData
                {
                    Title = commitMessage,
                    Body = MakeCommitDetails(updates),
                    Base = "master",
                    Head = branchName
                },
                RepositoryOwner = _settings.RepositoryOwner,
                RepositoryName = _settings.RepositoryName
            };

            await _github.OpenPullRequest(pr);
            await _git.Checkout("master");
        }

        private static void TryDelete(string tempDir)
        {
            Console.WriteLine($"Attempting delete of temp dir {tempDir}");

            try
            {
                Directory.Delete(tempDir, true);
            }
            catch (Exception)
            {
                Console.WriteLine("Delete failed. Continuing");
            }
        }

        private string MakeCommitMessage(List<PackageUpdate> updates, bool codeQuote)
        {
            var oldVersionsString = OldVersionsString(updates);
            var newVersion = updates[0].NewVersion;
            var packageId = updates[0].PackageId;
            if (codeQuote)
            {
                packageId = "`" + packageId + "`";
            }

            var pluralPackage = updates.Count == 1 ? "project" : "projects";
            return $"Automatic update of {packageId} from {oldVersionsString} to {newVersion} in {updates.Count} {pluralPackage}";
        }

        private string MakeCommitDetails(List<PackageUpdate> updates)
        {
            var oldVersionsString = OldVersionsString(updates);
            var newVersion = updates[0].NewVersion;
            var packageId = updates[0].PackageId;

            var paths = updates
                .Select(u => u.CurrentPackage.SourceFilePath.Replace(_tempDir, String.Empty))
                .Select(p => $"`{p}`");
            var pathsString = string.Join(",", paths);

            string updatePathsLine;

            if (updates.Count == 1)
            {
                updatePathsLine = $"Updated in 1 file: {pathsString}";
            }
            else
            {
                updatePathsLine = $"Updated in {updates.Count} files: {pathsString}";
            }

            return
                MakeCommitMessage(updates, true) + Environment.NewLine +
                $"NuKeeper has generated an update of `{packageId}` from version {oldVersionsString} to {newVersion}" + Environment.NewLine +
                updatePathsLine + Environment.NewLine +
                "This is an automated update. Merge only if it passes tests";
        }

        private static string OldVersionsString(IEnumerable<PackageUpdate> updates)
        {
            var oldVersions = updates
                .Select(u => u.OldVersion.ToString())
                .Distinct();

            return string.Join(",", oldVersions);
        }
    }
}