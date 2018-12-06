using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.NuGet;

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
            VersionChange allowedChange,
            UsePrerelease usePrerelease)
        {
            var allowBetas = ShouldAllowBetas(package, usePrerelease);

            var foundVersions = await _packageVersionsLookup.Lookup(package.Id, allowBetas, sources);
            return VersionChanges.MakeVersions(package.Version, foundVersions, allowedChange);
        }

        private static bool ShouldAllowBetas(PackageIdentity package, UsePrerelease usePrerelease)
        {
            switch (usePrerelease)
            {
                case UsePrerelease.Always:
                    return true;
                case UsePrerelease.Never:
                    return false;
                default:
                    return package.Version.IsPrerelease;
            }
        }
    }
}
