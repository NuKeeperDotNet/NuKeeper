using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Inspection.NuGetApi
{
    public interface IPackageUpdatesLookup
    {
        Task<IReadOnlyCollection<PackageUpdateSet>> FindUpdatesForPackages(
            IReadOnlyCollection<PackageInProject> packages,
            NuGetSources sources,
            VersionChange allowedChange,
            UsePrerelease usePrerelease);

        Task<IReadOnlyCollection<PackageUpdateSet>> FindDowngradeForPackages(
            IReadOnlyCollection<PackageInProject> packages,
            NuGetSources sources,
            VersionChange allowedChange,
            UsePrerelease usePrerelease); 

    }

}
