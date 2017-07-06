using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Nuget.Api
{
    public class PackageUpdatesLookup : IPackageUpdatesLookup
    {
        private readonly IApiPackageLookup _packageLookup;

        public PackageUpdatesLookup(IApiPackageLookup packageLookup)
        {
            _packageLookup = packageLookup;
        }

        public async Task<List<PackageUpdate>> FindUpdatesForPackages(List<NugetPackage> packages)
        {
            var result = new List<PackageUpdate>();

            foreach (var package in packages)
            {
                var serverVersion = await _packageLookup.LookupLatest(package.Id);

                if (serverVersion != null && serverVersion.Identity.Version > package.Version)
                {
                    result.Add(new PackageUpdate(package, serverVersion.Identity));
                }
            }

            return result;
        }
    }
}
