using System;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Git;
using NuKeeper.Github;
using NuKeeper.NuGet.Api;

namespace NuKeeper
{
    public class Program
    {
        public static int Main(string[] args)
        {
            TempFiles.DeleteExistingTempDirs();
                
            var settings = CommandLineParser.ReadSettings(args);

            if (settings == null)
            {
                Console.WriteLine("Exiting early...");
                return 1;
            }

            var lookups = new PackageUpdatesLookup(new ApiPackageLookup());
            var github = new GithubClient(settings);

            var repositoryDiscovery = new GithubRepositoryDiscovery(github, settings);
            var updateSelection = new PackageUpdateSelection(settings.MaxPullRequestsPerRepository);

            // get some storage space
            var tempDir = TempFiles.MakeUniqueTemporaryPath();
            var git = new LibGit2SharpDriver(tempDir, settings.GithubUser, settings.GithubToken);

            RunAll(repositoryDiscovery, lookups, updateSelection, github, git, tempDir)
                .GetAwaiter().GetResult();

            return 0;
        }

        private static async Task RunAll(
            GithubRepositoryDiscovery repositoryDiscovery,
            IPackageUpdatesLookup updatesLookup,
            IPackageUpdateSelection updateSelection,
            IGithub github,
            IGitDriver git,
            string tempDir)
        {
            var repositories = await repositoryDiscovery.GetRepositories();

            foreach (var repository in repositories)
            {
                try
                {

                    var repositoryUpdater = new RepositoryUpdater(updatesLookup, github, git, tempDir, updateSelection, repository);
                    await repositoryUpdater.Run();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
