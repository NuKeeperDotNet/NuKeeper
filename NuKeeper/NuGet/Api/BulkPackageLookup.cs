using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Packaging.Core;

namespace NuKeeper.NuGet.Api
{
    public class BulkPackageLookup: IBulkPackageLookup
    {
        private readonly IApiPackageLookup _packageLookup;
        private readonly PackageLookupResultReporter _lookupReporter;

        public BulkPackageLookup(
            IApiPackageLookup packageLookup, 
            PackageLookupResultReporter lookupReporter)
        {
            _packageLookup = packageLookup;
            _lookupReporter = lookupReporter;
        }

        public async Task<Dictionary<string, PackageSearchMedatadata>> FindVersionUpdates(
            IEnumerable<PackageIdentity> packages, VersionChange allowedChange)
        {
            var latestOfEach = packages
                .GroupBy(pi => pi.Id)
                .Select(HighestVersion);

            var lookupTasks = latestOfEach
                .Select(id => _packageLookup.FindVersionUpdate(id, allowedChange))
                .ToList();

            await Task.WhenAll(lookupTasks);

            var result = new Dictionary<string, PackageSearchMedatadata>();

            foreach (var lookupTask in lookupTasks)
            {
                var serverVersions = lookupTask.Result;
                ProcessLookupResult(serverVersions, result);
            }

            return result;
        }

        private void ProcessLookupResult(PackageLookupResult lookupResult, Dictionary<string, PackageSearchMedatadata> result)
        {
            var matchingVersion = lookupResult.Match;

            if (matchingVersion?.Identity?.Version != null)
            {
                _lookupReporter.Report(lookupResult);
                var packageId = matchingVersion.Identity.Id;
                result.Add(packageId, matchingVersion);
            }
        }

        private PackageIdentity HighestVersion(IEnumerable<PackageIdentity> packages)
        {
            return packages
                .OrderByDescending(p => p.Version)
                .FirstOrDefault();
        }
    }
}