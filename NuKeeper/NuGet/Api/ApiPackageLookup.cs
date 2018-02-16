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

            var orderedCandidates = versions
                .OrderByDescending(p => p.Identity.Version)
                .ToList();

            var major = HighestMatch(package.Version, orderedCandidates, VersionChange.Major);
            var minor = HighestMatch(package.Version, orderedCandidates, VersionChange.Minor);
            var patch = HighestMatch(package.Version, orderedCandidates, VersionChange.Patch);
            return new PackageLookupResult(allowedChange, major, minor, patch);
        }

        private static PackageSearchMedatadata HighestMatch(
            NuGetVersion current,
            IList<PackageSearchMedatadata> orderedCandidates,
            VersionChange allowedChange)
        {
            return orderedCandidates
                .FirstOrDefault(p => VersionChangeFilter.Filter(current, p.Identity.Version, allowedChange));
        }
    }
}
