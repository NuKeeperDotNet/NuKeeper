using System.Threading.Tasks;
using NuGet.Packaging.Core;

namespace NuKeeper.Inspection.NuGetApi
{
    public interface IApiPackageLookup
    {
        Task<PackageLookupResult> FindVersionUpdate(PackageIdentity package, VersionChange allowedChange);
    }
}
