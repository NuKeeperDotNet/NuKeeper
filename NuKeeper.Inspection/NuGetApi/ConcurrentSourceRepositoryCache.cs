using System.Collections.Concurrent;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;

namespace NuKeeper.Inspection.NuGetApi
{
    public class ConcurrentSourceRepositoryCache
    {
        private readonly ConcurrentDictionary<PackageSource, SourceRepository> _packageSources
            = new ConcurrentDictionary<PackageSource, SourceRepository>();

        public SourceRepository Get(PackageSource source)
        {
            return _packageSources.GetOrAdd(source, CreateSourceRepository);
        }

        private static SourceRepository CreateSourceRepository(PackageSource packageSource)
        {
            return new SourceRepository(packageSource, Repository.Provider.GetCoreV3());
        }
    }
}
