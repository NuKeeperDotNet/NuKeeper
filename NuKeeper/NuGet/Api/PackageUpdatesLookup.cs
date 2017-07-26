using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Api
{
    public class PackageUpdatesLookup : IPackageUpdatesLookup
    {
        private readonly IApiPackageLookup _packageLookup;

        public PackageUpdatesLookup(IApiPackageLookup packageLookup)
        {
            _packageLookup = packageLookup;
        }

        public async Task<List<PackageUpdateSet>> FindUpdatesForPackages(List<PackageInProject> packages)
        {
            var latestVersions = await BuildLatestVersionsDictionary(packages);
            var results = new List<PackageUpdateSet>();

            foreach (var packageId in latestVersions.Keys)
            {
                var latestVersion = latestVersions[packageId].Identity;

                var updatesForThisPackage = packages
                    .Where(p => p.Id == packageId && p.Version < latestVersion.Version)
                    .ToList();

                if (updatesForThisPackage.Count > 0)
                {
                    var updateSet = new PackageUpdateSet(latestVersion, updatesForThisPackage);
                    results.Add(updateSet);
                }
            }

            return results;
        }

        private async Task<Dictionary<string, IPackageSearchMetadata>> BuildLatestVersionsDictionary(IEnumerable<PackageInProject> packages)
        {
            var result = new Dictionary<string, IPackageSearchMetadata>();

            var packageIds = packages
                .Select(p => p.Id)
                .Distinct();

            var lookupTasks = packageIds
                .Select(id => _packageLookup.LookupLatest(id))
                .ToList();

            await Task.WhenAll(lookupTasks);

            foreach (var lookupTask in lookupTasks)
            {
                var serverVersion = lookupTask.Result;
                if (serverVersion?.Identity != null)
                {
                    var packageId = serverVersion.Identity.Id;
                    result.Add(packageId, serverVersion);
                    Console.WriteLine($"Found latest version of {packageId}: {serverVersion.Identity.Id} {serverVersion.Identity.Version}");
                }
            }

            return result;
        }
    }
}
