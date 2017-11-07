using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Api
{
    public class PackageUpdatesLookup : IPackageUpdatesLookup
    {
        private readonly IBulkPackageLookup _bulkPackageLookup;
        private readonly VersionChange _allowedChange;

        public PackageUpdatesLookup(IBulkPackageLookup bulkPackageLookup, UserSettings settings)
        {
            _bulkPackageLookup = bulkPackageLookup;
            _allowedChange = settings.AllowedChange;
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
                var identity = latestPackage.Match.Identity;

                var updatesForThisPackage = packages
                    .Where(p => p.Id == packageId && p.Version < identity.Version)
                    .ToList();

                if (updatesForThisPackage.Count > 0)
                {
                    var updateSet = new PackageUpdateSet(
                        identity,
                        latestPackage.Match.Source,
                        latestPackage.Highest.Identity.Version,
                        latestPackage.AllowedChange,
                        updatesForThisPackage);
                    results.Add(updateSet);
                }
            }

            return results;
        }
    }
}
