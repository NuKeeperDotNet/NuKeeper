using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace NuKeeper.Nuget
{
    public class NugetPackageLookup : INugetPackageLookup
    {
        public async Task<IPackageSearchMetadata> LookupLatest(string packageName)
        {
            var versions = await Lookup(packageName);
            return versions.FirstOrDefault();
        }

        private async Task<IEnumerable<IPackageSearchMetadata>> Lookup(string packageName)
        {
            var sourceRepository = BuildSourceRepository();
            var searchResource = await sourceRepository.GetResourceAsync<PackageSearchResource>();

            return await SearchForPackages(searchResource, packageName);
        }

        private static SourceRepository BuildSourceRepository()
        {
            var packageSource = new PackageSource("https://api.nuget.org/v3/index.json");

            var providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3()); // Add v3 API support

            return new SourceRepository(packageSource, providers);
        }

        private static async Task<IEnumerable<IPackageSearchMetadata>> SearchForPackages(PackageSearchResource searchResource, string packageName)
        {
            var logger = new NugetLogger();
            var filter = new SearchFilter(false);

            var packages = await searchResource
                .SearchAsync(packageName, filter, 0, 10, logger, CancellationToken.None);

            return packages;
        }
    }
}
