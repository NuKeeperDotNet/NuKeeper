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
        private readonly IPackageLookupResultReporter _lookupReporter;

        public BulkPackageLookup(
            IApiPackageLookup packageLookup,
            IPackageLookupResultReporter lookupReporter)
        {
            _packageLookup = packageLookup;
            _lookupReporter = lookupReporter;
        }

        public async Task<IDictionary<PackageIdentity, PackageLookupResult>> FindVersionUpdates(
            IEnumerable<PackageIdentity> packages,
            NuGetSources sources,
            VersionChange allowedChange,
            UsePrerelease usePrerelease
        )
        {
            var lookupTasks = packages
                .Distinct()
                .GroupBy(pi => (pi.Id, MaxVersion: GetMaxVersion(pi, allowedChange)))
                .Select(HighestVersion)
                .Select(id => new { Package = id, Update = _packageLookup.FindVersionUpdate(id, sources, allowedChange, usePrerelease) })
                .ToList();

            await Task.WhenAll(lookupTasks.Select(l => l.Update));

            var result = new Dictionary<PackageIdentity, PackageLookupResult>();

            foreach (var lookupTask in lookupTasks)
            {
                ProcessLookupResult(lookupTask.Package, lookupTask.Update.Result, result);
            }

            return result;
        }

        private static string GetMaxVersion(PackageIdentity pi, VersionChange allowedChange)
        {
            return allowedChange switch
            {
                VersionChange.Major => "X.X.X",
                VersionChange.Minor => $"{pi.Version.Major}.X.X",
                VersionChange.Patch => $"{pi.Version.Major}.{pi.Version.Minor}.X",
                VersionChange.None => $"{pi.Version.Major}.{pi.Version.Minor}.{pi.Version.Patch}",
                _ => throw new ArgumentOutOfRangeException(nameof(allowedChange)),
            };
        }

        private void ProcessLookupResult(
            PackageIdentity package,
            PackageLookupResult packageLookup,
            IDictionary<PackageIdentity, PackageLookupResult> result
        )
        {
            var selectedVersion = packageLookup.Selected();

            if (selectedVersion?.Identity?.Version != null)
            {
                _lookupReporter.Report(packageLookup);
                result.Add(package, packageLookup);
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
