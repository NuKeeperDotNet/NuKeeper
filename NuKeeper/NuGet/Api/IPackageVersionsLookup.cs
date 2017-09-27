using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.NuGet.Api
{
    public interface IPackageVersionsLookup
    {
        Task<IEnumerable<PackageSearchMedatadataWithSource>> Lookup(string packageName);
    }
}