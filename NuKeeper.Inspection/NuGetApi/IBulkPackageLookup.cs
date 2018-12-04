using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.NuGet;

namespace NuKeeper.Inspection.NuGetApi
{
    public interface IBulkPackageLookup
    {
        Task<IDictionary<string, PackageLookupResult>> FindVersionUpdates(
            IEnumerable<PackageIdentity> packages,
            NuGetSources sources,
            VersionChange allowedChange,
            UsePrerelease usePrerelease);
    }
}
