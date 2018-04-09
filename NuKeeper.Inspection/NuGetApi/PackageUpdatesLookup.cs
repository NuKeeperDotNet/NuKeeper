using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.NuGetApi
{
    public class PackageUpdatesLookup : IPackageUpdatesLookup
    {
        private readonly IBulkPackageLookup _bulkPackageLookup;
        private readonly VersionChange _allowedChange;

        public PackageUpdatesLookup(IBulkPackageLookup bulkPackageLookup, VersionChange allowedChange)
        {
            _bulkPackageLookup = bulkPackageLookup;
            _allowedChange = allowedChange;
        }

        public async Task<List<PackageUpdateSet>> FindUpdatesForPackages(IReadOnlyCollection<PackageInProject> packages)
        {
            var packageIds = packages
                .Select(p => p.Identity)
                .Distinct();

            var latestVersions = await _bulkPackageLookup.FindVersionUpdates(packageIds, _allowedChange);

            var results = new List<PackageUpdateSet>();

            foreach (var packageId in latestVersions.Keys)
            {
                var latestPackage = latestVersions[packageId];
                var matchVersion = latestPackage.Selected().Identity.Version;

                var updatesForThisPackage = packages
                    .Where(p => p.Id == packageId && p.Version < matchVersion)
                    .ToList();

                if (updatesForThisPackage.Count > 0)
                {
                    var updateSet = new PackageUpdateSet(latestPackage, updatesForThisPackage);
                    results.Add(updateSet);
                }
            }

            return results;
        }
    }
}
