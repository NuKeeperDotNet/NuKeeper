using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.NuGetApi
{
    public interface IPackageUpdatesLookup
    {
        Task<List<PackageUpdateSet>> FindUpdatesForPackages(IReadOnlyCollection<PackageInProject> packages);
    }
}
