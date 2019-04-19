using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.NuGetApi;

namespace NuKeeper.Inspection.NuGetApi
{
    public class BulkPackageLookup : IBulkPackageLookup
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

        public async Task<IDictionary<string, PackageLookupResult>> FindVersionUpdates(
            IEnumerable<PackageIdentity> packages,
            NuGetSources sources,
            VersionChange allowedChange,
            UsePrerelease usePrerelease)
        {
            var latestOfEach = packages
                .GroupBy(pi => pi.Id.ToUpperInvariant())
                .Select(HighestVersion);

            var lookupTasks = latestOfEach
                .Select(id => _packageLookup.FindVersionUpdate(id, sources, allowedChange, usePrerelease))
                .ToList();

            await Task.WhenAll(lookupTasks);

            var result = new Dictionary<string, PackageLookupResult>(StringComparer.OrdinalIgnoreCase);

            foreach (var lookupTask in lookupTasks)
            {
                var serverVersions = lookupTask.Result;
                ProcessLookupResult(serverVersions, result);
            }

            return result;
        }

        private void ProcessLookupResult(PackageLookupResult packageLookup, IDictionary<string, PackageLookupResult> result)
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
