using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Inspection.NuGetApi
{
    public class PackageVersionsLookup : IPackageVersionsLookup
    {
        private readonly ILogger _logger;

        public PackageVersionsLookup(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<PackageSearchMedatadata>> Lookup(
            string packageName,
            NuGetSources sources)
        {
            var tasks = sources.Items.Select(s => RunFinderForSource(packageName, s));

            var results = await Task.WhenAll(tasks);

            return results
                .SelectMany(r => r)
                .Where(p => p?.Identity?.Version != null)
                .ToList();
        }

        private async Task<IEnumerable<PackageSearchMedatadata>> RunFinderForSource(string packageName, string source)
        {
            var sourceRepository = BuildSourceRepository(source);
            var metadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();
            var metadatas = await FindPackage(metadataResource, packageName);
            return metadatas.Select(m => BuildPackageData(source, m));
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

        private static PackageSearchMedatadata BuildPackageData(string source, IPackageSearchMetadata metadata)
        {
            var deps = metadata.DependencySets
                .SelectMany(set => set.Packages)
                .Distinct();

            return new PackageSearchMedatadata(metadata.Identity, source, metadata.Published, deps);
        }
    }
}
