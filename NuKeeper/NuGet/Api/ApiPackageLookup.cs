using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Settings = NuKeeper.Configuration.Settings;

namespace NuKeeper.NuGet.Api
{
    public class ApiPackageLookup : IApiPackageLookup
    {
        private readonly ILogger _logger;
        private readonly string[] _sources;

        public ApiPackageLookup(ILogger logger, Settings settings)
        {
            _logger = logger;
            _sources = settings.NuGetSources;
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
            var results = await Task.WhenAll(_sources.Select(source => RunFinderForSource(packageName, source)));
            return results.SelectMany(r => r);
        }

        private async Task<IEnumerable<IPackageSearchMetadata>> RunFinderForSource(string packageName, string source)
        {
            var sourceRepository = BuildSourceRepository(source);
            var metadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();
            return await FindPackage(metadataResource, packageName);
        }

        private static SourceRepository BuildSourceRepository(string source)
        {
            var packageSource = new PackageSource(source);

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