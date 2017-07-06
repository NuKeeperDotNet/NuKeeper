using System;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.RepositoryInspection;

namespace NuKeeper
{
    public class Engine
    {
        public async Task Run(Uri gitUrl)
        {
            Console.WriteLine($"Git url: {gitUrl}");

            // clone
            var tempDir = FileHelper.MakeUniqueTemporaryPath();

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
        }
    }
}