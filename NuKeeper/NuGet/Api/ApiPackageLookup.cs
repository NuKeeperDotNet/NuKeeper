using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace NuKeeper.NuGet.Api
{
    public class ApiPackageLookup : IApiPackageLookup
    {
        private readonly ILogger _logger;

        public ApiPackageLookup(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<IPackageSearchMetadata> LookupLatest(string packageName)
        {
            var versions = await Lookup(packageName);
            return versions
                .OrderByDescending(p => p?.Identity?.Version)
                .FirstOrDefault();
        }

        private async Task<IEnumerable<IPackageSearchMetadata>> Lookup(string packageName)
        {
            var sourceRepository = BuildSourceRepository();
            var metadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();
            return await FindPackage(metadataResource, packageName);
        }

        private static SourceRepository BuildSourceRepository()
        {
            var packageSource = new PackageSource("https://api.nuget.org/v3/index.json");

            var providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3()); // Add v3 API support

            return new SourceRepository(packageSource, providers);
        }

        private async Task<IEnumerable<IPackageSearchMetadata>> FindPackage(
            PackageMetadataResource metadataResource, string packageName)
        { 
            return await metadataResource
                .GetMetadataAsync(packageName, false, false, _logger, CancellationToken.None);
        }
    }
}
