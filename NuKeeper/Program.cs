using System;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Engine;
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

            RunAll(repositoryDiscovery, lookups, github)
                .GetAwaiter().GetResult();

            return 0;
        }

        private static async Task RunAll(
            GithubRepositoryDiscovery repositoryDiscovery,
            IPackageUpdatesLookup lookups,
            IGithub github)
        {
            var repositories = await repositoryDiscovery.GetRepositories();

            foreach (var repository in repositories)
            {
                try
                {
                    var repositoryUpdater = new RepositoryUpdater(lookups, github, repository);
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
