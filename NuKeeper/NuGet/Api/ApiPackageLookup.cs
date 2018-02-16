using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace NuKeeper.NuGet.Api
{
    public class ApiPackageLookup : IApiPackageLookup
    {
        private readonly IPackageVersionsLookup _packageVersionsLookup;

        public ApiPackageLookup(IPackageVersionsLookup packageVersionsLookup)
        {
            _packageVersionsLookup = packageVersionsLookup;
        }

        public async Task<PackageLookupResult> FindVersionUpdate(
            PackageIdentity package, VersionChange allowedChange)
        {
            var versions = await _packageVersionsLookup.Lookup(package.Id);

            var versionsList = versions.ToList();

            var highest = HighestThatMatchesFilter(package.Version, versionsList, VersionChange.Major);
            var highestThatMatchesFilter = HighestThatMatchesFilter(package.Version, versionsList, allowedChange);

            return new PackageLookupResult(allowedChange, highest, highestThatMatchesFilter);
        }

        private static PackageSearchMedatadata HighestThatMatchesFilter(
            NuGetVersion current,
            IList<PackageSearchMedatadata> candidates,
            VersionChange allowedChange)
        {
            var orderedCandidates = candidates
                .OrderByDescending(p => p.Identity.Version)
                .ToList();

            return orderedCandidates
                .FirstOrDefault(p => VersionChangeFilter.Filter(current, p.Identity.Version, allowedChange));
        }
    }
}
