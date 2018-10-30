using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.NuGetApi
{
    public interface IPackageUpdatesLookup
    {
        Task<IReadOnlyCollection<PackageUpdateSet>> FindUpdatesForPackages(
            IReadOnlyCollection<PackageInProject> packages,
            NuGetSources sources,
            VersionChange allowedChange);
    }
}
