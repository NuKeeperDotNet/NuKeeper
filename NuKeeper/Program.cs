using System;
using System.Collections.Generic;
using System.Linq;
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
            IEnumerable<RepositoryModeSettings> repositorySettings = Enumerable.Empty<RepositoryModeSettings>();
            if (settings.Mode == Settings.OrganisationMode)
            {
                repositorySettings = repositoryDiscovery.FromOrganisation(settings.Organisation);
            }
            else if(settings.Mode == Settings.RepositoryMode)
            {
                repositorySettings = new[] {settings.Repository};
            }

            foreach (var repository in repositorySettings)
            {
                var engine = new RepositoryUpdater(lookups, github, repository);
                engine.Run()
                    .GetAwaiter().GetResult();
            }

            return 0;
        }
    }
}
