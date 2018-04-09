using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Packaging.Core;

namespace NuKeeper.Inspection.NuGetApi
{
    public interface IBulkPackageLookup
    {
        Task<Dictionary<string, PackageLookupResult>> FindVersionUpdates(
            IEnumerable<PackageIdentity> packages,
            VersionChange allowedChange);
    }
}
