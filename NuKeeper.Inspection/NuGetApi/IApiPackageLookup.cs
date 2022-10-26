using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.NuGetApi;

namespace NuKeeper.Inspection.NuGetApi
{
    public interface IApiPackageLookup
    {
        Task<PackageLookupResult> FindVersionUpdate(
            PackageIdentity package,
            NuGetSources sources,
            VersionChange allowedChange,
            UsePrerelease usePrerelease,
            bool throwOnGitError);
    }
}
