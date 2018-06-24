using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Inspection.NuGetApi
{
    public interface IBulkPackageLookup
    {
        Task<Dictionary<string, PackageLookupResult>> FindVersionUpdates(
            IEnumerable<PackageIdentity> packages,
            NuGetSources sources,
            VersionChange allowedChange);
    }
}
