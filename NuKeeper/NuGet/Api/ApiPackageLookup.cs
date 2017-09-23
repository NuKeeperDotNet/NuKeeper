using System.Linq;
using System.Threading.Tasks;

namespace NuKeeper.NuGet.Api
{
    public class ApiPackageLookup : IApiPackageLookup
    {
        private readonly IPackageVersionsLookup _packageVersionsLookup;

        public ApiPackageLookup(IPackageVersionsLookup packageVersionsLookup)
        {
            _packageVersionsLookup = packageVersionsLookup;
        }

        public async Task<PackageSearchMedatadataWithSource> LookupLatest(string packageName)
        {
            var versions = await _packageVersionsLookup.Lookup(packageName);
            return versions.FirstOrDefault();
        }
    }
}