using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.NuGet.Api
{
    public interface IBulkPackageLookup
    {
        Task<Dictionary<string, PackageSearchMedatadataWithSource>> LatestVersions(IEnumerable<string> packageIds);
    }
}