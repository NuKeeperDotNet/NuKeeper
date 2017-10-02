using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuKeeper.Logging;

namespace NuKeeper.NuGet.Api
{
    public class BulkPackageLookup: IBulkPackageLookup
    {
        private readonly IApiPackageLookup _packageLookup;
        private readonly INuKeeperLogger _logger;

        public BulkPackageLookup(IApiPackageLookup packageLookup, INuKeeperLogger logger)
        {
            _packageLookup = packageLookup;
            _logger = logger;
        }

        public async Task<Dictionary<string, PackageSearchMedatadataWithSource>> LatestVersions(IEnumerable<PackageIdentity> packages)
        {
            var latestOfEach = packages
                .GroupBy(pi => pi.Id)
                .Select(HighestVersion);

            var lookupTasks = latestOfEach
                .Select(id => _packageLookup.FindVersionUpdate(id))
                .ToList();

            await Task.WhenAll(lookupTasks);

            var result = new Dictionary<string, PackageSearchMedatadataWithSource>();

            foreach (var lookupTask in lookupTasks)
            {
                var serverVersion = lookupTask.Result;
                if (serverVersion?.Identity?.Version != null)
                {
                    var packageId = serverVersion.Identity.Id;
                    _logger.Verbose($"Found latest version of {packageId}: {serverVersion.Identity.Version}");
                    result.Add(packageId, serverVersion);
                }
            }

            return result;
        }

        private PackageIdentity HighestVersion(IEnumerable<PackageIdentity> packages)
        {
            return packages
                .OrderByDescending(p => p.Version)
                .FirstOrDefault();
        }
    }
}