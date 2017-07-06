using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Github;
using NuKeeper.Git;
using NuKeeper.Nuget.Api;
using NuKeeper.Nuget.Process;
using NuKeeper.RepositoryInspection;

namespace NuKeeper
{
    public class Engine
    {
        private readonly IPackageUpdatesLookup _packageLookup;
        private readonly Settings _settings;

        public Engine(IPackageUpdatesLookup packageLookup, Settings settings)
        {
            _packageLookup = packageLookup;
            _settings = settings;
        }

        public async Task Run()
        {
            // get some storage space
            var tempDir = TempFiles.MakeUniqueTemporaryPath();
            Console.WriteLine($"Using temp dir: {tempDir}");

            // clone the repo
            Console.WriteLine($"Git url: {_settings.GitUri}");
            var git = new GitDriver(tempDir);
            await git.Clone(_settings.GitUri);

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

            Console.WriteLine("Commiting");
            var commitMessage = MakeCommitMessage(applicable);
            await git.Commit(commitMessage);

            Console.WriteLine($"Pushing branch '{branchName}'");
            await git.Push("origin", branchName);

            Console.WriteLine($"Making PR on '{_settings.GithubBaseUri} {_settings.RepositoryOwner} {_settings.RepositoryName}'");

            // open github PR
            var pr = new OpenPullRequestRequest
            {
                Data = new PullRequestData
                {
                    Title = commitMessage,
                    Body = MakeCommitDetails(applicable),
                    Base = "master",
                    Head = branchName
                },
                RepositoryOwner = _settings.RepositoryOwner,
                RepositoryName = _settings.RepositoryName
            };

            var github = new GithubClient(_settings);
            await github.OpenPullRequest(pr);

            // delete the temp folder
            TryDelete(tempDir);
            Console.WriteLine("Done");
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

        private string MakeCommitMessage(PackageUpdate update)
        {
            return $"Automatic update of {update.PackageId} from {update.OldVersion} to {update.NewVersion}";
        }

        private string MakeCommitDetails(PackageUpdate update)
        {
            return MakeCommitMessage(update) + Environment.NewLine +
            $"NuKeeper has generated an update of `{update.PackageId}` from version {update.OldVersion} to {update.NewVersion}" + Environment.NewLine +
            "This is an automated update. Merge if it passes tests";
        }
    }
}