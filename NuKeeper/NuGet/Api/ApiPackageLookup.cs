using System.Linq;
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

        public async Task<VersionUpdate> FindVersionUpdate(
            PackageIdentity package, VersionChange allowedChange)
        {
            var filter = VersionChangeFilter.FilterFor(allowedChange);

            var versions = await _packageVersionsLookup.Lookup(package.Id);
            var ordered = versions
                .OrderByDescending(p => p.Identity.Version)
                .ToList();

            var highestVersion = ordered.FirstOrDefault();
            var highestMatch = ordered.FirstOrDefault(p => filter(package.Version, p.Identity.Version));
            
            return new VersionUpdate
            {
                Highest = highestVersion,
                HighestMatch = highestMatch
            };
        }
    }
}