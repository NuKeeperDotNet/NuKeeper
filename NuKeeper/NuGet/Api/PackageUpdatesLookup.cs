using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Api
{
    public class PackageUpdatesLookup : IPackageUpdatesLookup
    {
        private readonly IApiPackageLookup _packageLookup;

        public PackageUpdatesLookup(IApiPackageLookup packageLookup)
        {
            _packageLookup = packageLookup;
        }

        public async Task<List<PackageUpdate>> FindUpdatesForPackages(List<NuGetPackage> packages)
        {
            var result = new List<PackageUpdate>();

            var latestVersions = await BuildVersionsDictionary(packages);

            foreach (var package in packages)
            {
                var serverVersion = latestVersions[package.Id];

                if (serverVersion != null && serverVersion.Identity.Version > package.Version)
                {
                    result.Add(new PackageUpdate(package, serverVersion.Identity));
                }
            }

            return result;
        }

        private async Task<Dictionary<string, IPackageSearchMetadata>> BuildVersionsDictionary(IEnumerable<NuGetPackage> packages)
        {
            var result = new Dictionary<string, IPackageSearchMetadata>();

            foreach (var packageId in packages.GroupBy(p => p.Id).Select(p => p.Key))
            {
                var serverVersion = await _packageLookup.LookupLatest(packageId);

                result.Add(packageId, serverVersion);
            }

            return result;
        }
    }
}
