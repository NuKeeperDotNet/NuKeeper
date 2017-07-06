using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Nuget.Api;
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
            Console.WriteLine($"Git url: {gitUrl}");

            // clone
            var tempDir = TempFiles.MakeUniqueTemporaryPath();

            Console.WriteLine($"Using temp dir: {tempDir}");


            var git = new Git.Git(tempDir);
            await git.Clone(gitUrl);

            Console.WriteLine("Git clone complete");

            // scan
            var repoScanner = new RepositoryScanner();
            var packages = repoScanner.FindAllNugetPackages(tempDir)
                .ToList();


            var packageNames = string.Join(",", packages.Take(10).Select(p => p.Id));

            Console.WriteLine($"Found {packages.Count} packages: {packageNames}");

            // look for updates
            var updates = await _packageLookup.FindUpdatesForPackages(packages);
            var updateDetails = updates.Take(10).Select(p => $"{p.CurrentPackage.Id} from {p.OldVersion} to {p.NewVersion}");

            Console.WriteLine($"Found {updates.Count} updates: {string.Join(",", updateDetails)}");
        }
    }
}