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

        public async Task<IReadOnlyCollection<PackageSearchMedatadata>> Lookup(
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

        private async Task<IEnumerable<PackageSearchMedatadata>> RunFinderForSource(
            string packageName, bool includePrerelease, PackageSource source)
        {
            var sourceRepository = _packageSources.Get(source);
            try
            {
                var metadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();
                var metadatas = await FindPackage(metadataResource, packageName, includePrerelease);
                return metadatas.Select(m => BuildPackageData(source, m));
            }
            catch (Exception ex)
            {
                _nuKeeperLogger.Error($"Error getting {packageName} from {source}", ex);
                if (ex.InnerException != null)
                {
                    _nuKeeperLogger.Error("Inner Exception", ex.InnerException);
                }

                return Enumerable.Empty<PackageSearchMedatadata>();
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

        private static PackageSearchMedatadata BuildPackageData(PackageSource source, IPackageSearchMetadata metadata)
        {
            var deps = metadata.DependencySets
                .SelectMany(set => set.Packages)
                .Distinct();

            return new PackageSearchMedatadata(metadata.Identity, source, metadata.Published, deps);
        }
    }
}
