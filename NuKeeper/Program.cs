using System;
using System.Linq;
using NuKeeper.Configuration;
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
            var engine = new Engine(lookups, github, settings.Repository);
            engine.Run()
                .GetAwaiter().GetResult();

            return 0;
        }
    }
}
