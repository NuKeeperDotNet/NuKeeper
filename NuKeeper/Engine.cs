using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Nuget.Api;
using NuKeeper.Nuget.Process;
using NuKeeper.RepositoryInspection;

namespace NuKeeper
{
    public class Engine
    {
        private readonly IPackageUpdatesLookup _packageLookup;

        public Engine(IPackageUpdatesLookup packageLookup)
        {
            _packageLookup = packageLookup;
        }

        public async Task Run(Uri gitUrl)
        {
            // get some storage space
            var tempDir = TempFiles.MakeUniqueTemporaryPath();
            Console.WriteLine($"Using temp dir: {tempDir}");

            // clone the repo
            Console.WriteLine($"Git url: {gitUrl}");
            var git = new Git.Git(tempDir);
            await git.Clone(gitUrl);

            Console.WriteLine("Git clone complete");

            // scan for nuget packages
            var repoScanner = new RepositoryScanner();
            var packages = repoScanner.FindAllNugetPackages(tempDir)
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

            // pick an update 
            var applicable = updates.First();

            // branch
            var branchName = $"nukeeper-update-{applicable.PackageId}-from-{applicable.OldVersion}-to-{applicable.NewVersion}";
            await git.Checkout(branchName);

            Console.WriteLine($"Using branch '{branchName}'");

            var nugetUpdater = new NugetUpdater();
            await nugetUpdater.UpdatePackage(applicable);

            var commitMessage = MakeCommitMessage(applicable);
            await git.Commit(commitMessage);

            await git.Push("origin", branchName);
        }

        private string MakeCommitMessage(PackageUpdate update)
        {
            return $"Automatic update of {update.PackageId} from {update.OldVersion} to {update.NewVersion}" + Environment.NewLine +
            $"Nukeeper has generated an update of `{update.PackageId}` from version {update.OldVersion} to {update.NewVersion}" + Environment.NewLine +
            "This is an automated update. Merge if it passes tests";
        }
    }
}