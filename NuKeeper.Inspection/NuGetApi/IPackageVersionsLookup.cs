using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.NuGet;

namespace NuKeeper.Inspection.NuGetApi
{
    public interface IPackageVersionsLookup
    {
        Task<IReadOnlyCollection<PackageSearchMedatadata>> Lookup(
            string packageName, bool includePrerelease,
            NuGetSources sources);
    }
}
