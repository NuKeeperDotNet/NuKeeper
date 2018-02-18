using System.Threading.Tasks;
using NuGet.Packaging.Core;

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
            var foundVersions = await _packageVersionsLookup.Lookup(package.Id);
            return VersionChanges.MakeVersions(package.Version, foundVersions, allowedChange);
        }
    }
}
