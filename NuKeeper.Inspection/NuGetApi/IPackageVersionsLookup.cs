using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Inspection.NuGetApi
{
    public interface IPackageVersionsLookup
    {
        Task<IReadOnlyCollection<PackageSearchMedatadata>> Lookup(
            string packageName, NuGetSources sources);
    }
}
