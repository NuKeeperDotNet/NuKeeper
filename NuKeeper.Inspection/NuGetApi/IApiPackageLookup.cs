using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Inspection.NuGetApi
{
    public interface IApiPackageLookup
    {
        Task<PackageLookupResult> FindVersionUpdate(
            PackageIdentity package,
            NuGetSources sources,
            VersionChange allowedChange);
    }
}
