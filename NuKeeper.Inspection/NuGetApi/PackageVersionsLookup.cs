using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.NuGetApi;

namespace NuKeeper.Inspection.NuGetApi
{
    public class PackageVersionsLookup : IPackageVersionsLookup
    {
        private readonly ILogger _nuGetLogger;
        private readonly INuKeeperLogger _nuKeeperLogger;
        private readonly ConcurrentSourceRepositoryCache _packageSources
            = new ConcurrentSourceRepositoryCache();

        public PackageVersionsLookup(ILogger nuGetLogger, INuKeeperLogger nuKeeperLogger)
        {
            _nuGetLogger = nuGetLogger;
            _nuKeeperLogger = nuKeeperLogger;
        }

        public async Task<IReadOnlyCollection<PackageSearchMetadata>> Lookup(
            string packageName, bool includePrerelease,
            NuGetSources sources)
        {
            var tasks = sources.Items.Select(s => RunFinderForSource(packageName, includePrerelease, s));

            var results = await Task.WhenAll(tasks);

            return results
                .SelectMany(r => r)
                .Where(p => p?.Identity?.Version != null)
                .ToList();
        }

        private async Task<IEnumerable<PackageSearchMetadata>> RunFinderForSource(
            string packageName, bool includePrerelease, PackageSource source)
        {
            var sourceRepository = _packageSources.Get(source);
            try
            {
                var metadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();
                var metadatas = await FindPackage(metadataResource, packageName, includePrerelease);
                return metadatas.Select(m => BuildPackageData(source, m));
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _nuKeeperLogger.Normal($"Getting {packageName} from {source} returned exception: {ex.Message}");
                return Enumerable.Empty<PackageSearchMetadata>();
            }
        }

        private async Task<IEnumerable<IPackageSearchMetadata>> FindPackage(
            PackageMetadataResource metadataResource, string packageName, bool includePrerelease)
        {
            using (var cacheContext = new SourceCacheContext())
            {
                return await metadataResource
                    .GetMetadataAsync(packageName, includePrerelease, false,
                        cacheContext, _nuGetLogger, CancellationToken.None);
            }
        }

        private static PackageSearchMetadata BuildPackageData(PackageSource source, IPackageSearchMetadata metadata)
        {
            var deps = metadata.DependencySets
                .SelectMany(set => set.Packages)
                .Distinct();

            return new PackageSearchMetadata(metadata.Identity, source, metadata.Published, deps);
        }
    }
}
