using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Api
{
    public class PackageUpdatesLookup : IPackageUpdatesLookup
    {
        private readonly BulkPackageSearch _bulkSearch;

        public PackageUpdatesLookup(IApiPackageLookup packageLookup)
        {
            _bulkSearch = new BulkPackageSearch(packageLookup);
        }

        public async Task<List<PackageUpdateSet>> FindUpdatesForPackages(IEnumerable<PackageInProject> packages)
        {
            var packageIds = packages
                .Select(p => p.Id)
                .Distinct();

            var latestVersions = await _bulkSearch.LatestVersions(packageIds);

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
    }
}
