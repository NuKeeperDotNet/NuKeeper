using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Github;
using NuKeeper.Nuget.Api;

namespace NuKeeper
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var settings = CommandLineParser.ReadSettings(args);

            if (settings == null)
            {
                return 1;
            }

            var lookups = new PackageUpdatesLookup(new ApiPackageLookup());
            var github = new GithubClient(settings);

            var repositoryDiscovery = new GithubRepositoryDiscovery(github);

            var repositories = repositoryDiscovery.GetRepositories(settings);

            RunAll(repositories, lookups, github).GetAwaiter().GetResult();

            return 0;
        }

        private static async Task RunAll(IEnumerable<RepositoryModeSettings> repositories,
            PackageUpdatesLookup lookups,
            GithubClient github)
        {
            foreach (var repository in repositories)
            {
                var engine = new RepositoryUpdater(lookups, github, repository);
                await engine.Run();
            }
        }
    }
}
