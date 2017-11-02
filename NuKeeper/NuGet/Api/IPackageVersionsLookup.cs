using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.NuGet.Api
{
    public interface IPackageVersionsLookup
    {
        Task<IEnumerable<PackageSearchMedatadata>> Lookup(string packageName);
    }
}