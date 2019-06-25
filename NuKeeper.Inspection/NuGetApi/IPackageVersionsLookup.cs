using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.NuGetApi;

namespace NuKeeper.Inspection.NuGetApi
{
    public interface IPackageVersionsLookup
    {
        Task<IReadOnlyCollection<PackageSearchMetadata>> Lookup(
            string packageName, bool includePrerelease,
            NuGetSources sources);
    }
}
