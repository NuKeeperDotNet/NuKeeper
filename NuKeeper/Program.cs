using System;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Git;
using NuKeeper.Github;
using NuKeeper.NuGet.Api;
using SimpleInjector;

namespace NuKeeper
{
    public class Program
    {
        public static int Main(string[] args)
        {
            TempFiles.DeleteExistingTempDirs();
                
            var settings = SettingsParser.ReadSettings(args);

            if(settings == null)
            {
                Console.WriteLine("Exiting early...");
                return 1;
            }

            var container = RegisterContainer(settings);

            TempFiles.DeleteExistingTempDirs();

            if (container.GetInstance<Settings>() == null)
            {
            }

            // get some storage space
            var tempDir = TempFiles.MakeUniqueTemporaryPath();

            RunAll(container.GetInstance<IGithubRepositoryDiscovery>(),
                container.GetInstance<IPackageUpdatesLookup>(),
                container.GetInstance<IPackageUpdateSelection>(),
                container.GetInstance<IGithub>(),
                tempDir,
                container.GetInstance<Settings>().GithubToken)
                .GetAwaiter().GetResult();

            return 0;
        }

        private static Container RegisterContainer(Settings settings)
        {
            var container = new Container();

            container.Register(() => settings, Lifestyle.Singleton);
            container.Register<IGithub, OctokitClient>();
            container.Register<IGithubRepositoryDiscovery, GithubRepositoryDiscovery>();
            container.Register<IPackageUpdateSelection, PackageUpdateSelection>();
            container.Register<IPackageUpdatesLookup, PackageUpdatesLookup>();
            container.Register<IBulkPackageLookup, BulkPackageLookup>();
            container.Register<IApiPackageLookup, ApiPackageLookup>();

            return container;
        }

        private static async Task RunAll(
            IGithubRepositoryDiscovery repositoryDiscovery,
            IPackageUpdatesLookup updatesLookup,
            IPackageUpdateSelection updateSelection,
            IGithub github,
            string tempDir,
            string githubToken)
        {
            var githubUser = await github.GetCurrentUser();
            Console.WriteLine($"Read github user '{githubUser}'");

            var git = new LibGit2SharpDriver(tempDir, githubUser, githubToken);

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
                    Console.WriteLine($"Repo failed {e.GetType().Name}: {e.Message}");
                }
            }
        }
    }
}
