using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Inspection.NuGetApi
{
    public class PackageVersionsLookup : IPackageVersionsLookup
    {
        private readonly ILogger _nuGetLogger;
        private readonly INuKeeperLogger _nuKeeperLogger;

        public PackageVersionsLookup(ILogger nuGetLogger, INuKeeperLogger nuKeeperLogger)
        {
            _nuGetLogger = nuGetLogger;
            _nuKeeperLogger = nuKeeperLogger;
        }

        public async Task<IReadOnlyCollection<PackageSearchMedatadata>> Lookup(
            string packageName,
            NuGetSources sources)
        {
            var tasks = sources.Sources.Select(s => RunFinderForSource(packageName, s));

            var results = await Task.WhenAll(tasks);

            return results
                .SelectMany(r => r)
                .Where(p => p?.Identity?.Version != null)
                .ToList();
        }

        private async Task<IEnumerable<PackageSearchMedatadata>> RunFinderForSource(string packageName, PackageSource source)
        {
            var sourceRepository = BuildSourceRepository(source);
            try
            {
                var metadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();
                var metadatas = await FindPackage(metadataResource, packageName);
                return metadatas.Select(m => BuildPackageData(source, m));
            }
            catch (Exception ex)
            {
                _nuKeeperLogger.Error($"Error getting {packageName} from {source}", ex);
                return Enumerable.Empty<PackageSearchMedatadata>();
            }
        }

        private static SourceRepository BuildSourceRepository(PackageSource packageSource)
        {
            var providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3()); // Add v3 API support

            return new SourceRepository(packageSource, providers);
        }

        private async Task<IEnumerable<IPackageSearchMetadata>> FindPackage(
            PackageMetadataResource metadataResource, string packageName)
        {
            using (var cacheContext = new SourceCacheContext())
            {
                return await metadataResource
                    .GetMetadataAsync(packageName, false, false, cacheContext, _nuGetLogger, CancellationToken.None);
            }
        }

        private static PackageSearchMedatadata BuildPackageData(PackageSource source, IPackageSearchMetadata metadata)
        {
            var deps = metadata.DependencySets
                .SelectMany(set => set.Packages)
                .Distinct();

            return new PackageSearchMedatadata(metadata.Identity, source, metadata.Published, deps);
        }
    }
}
