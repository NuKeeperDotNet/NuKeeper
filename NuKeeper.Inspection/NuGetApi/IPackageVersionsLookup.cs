using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.Inspection.NuGetApi
{
    public interface IPackageVersionsLookup
    {
        Task<IEnumerable<PackageSearchMedatadata>> Lookup(string packageName);
    }
}
