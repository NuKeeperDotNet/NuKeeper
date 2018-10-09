using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Inspection.NuGetApi
{
    public class ApiPackageLookup : IApiPackageLookup
    {
        private readonly IPackageVersionsLookup _packageVersionsLookup;

        public ApiPackageLookup(IPackageVersionsLookup packageVersionsLookup)
        {
            _packageVersionsLookup = packageVersionsLookup;
        }

        public async Task<PackageLookupResult> FindVersionUpdate(
            PackageIdentity package,
            NuGetSources sources,
            VersionChange allowedChange)
        {
            var allowBetas = package.Version.IsPrerelease;
            var foundVersions = await _packageVersionsLookup.Lookup(package.Id, allowBetas, sources)
                .ConfigureAwait(false);
            return VersionChanges.MakeVersions(package.Version, foundVersions, allowedChange);
        }
    }
}
