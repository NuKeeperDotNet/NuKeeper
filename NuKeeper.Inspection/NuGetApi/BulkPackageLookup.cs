using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Inspection.NuGetApi
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

        public async Task<Dictionary<string, PackageLookupResult>> FindVersionUpdates(
            IEnumerable<PackageIdentity> packages,
            NuGetSources sources,
            VersionChange allowedChange)
        {
            var latestOfEach = packages
                .GroupBy(pi => pi.Id)
                .Select(HighestVersion);

            var lookupTasks = latestOfEach
                .Select(id => _packageLookup.FindVersionUpdate(id, sources, allowedChange))
                .ToList();

            await Task.WhenAll(lookupTasks);

            var result = new Dictionary<string, PackageLookupResult>();

            foreach (var lookupTask in lookupTasks)
            {
                var serverVersions = lookupTask.Result;
                ProcessLookupResult(serverVersions, result);
            }

            return result;
        }

        private void ProcessLookupResult(PackageLookupResult packageLookup, Dictionary<string, PackageLookupResult> result)
        {
            var selectedVersion = packageLookup.Selected();

            if (selectedVersion?.Identity?.Version != null)
            {
                _lookupReporter.Report(packageLookup);
                var packageId = selectedVersion.Identity.Id;
                result.Add(packageId, packageLookup);
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
