using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Inspection
{
    public interface IUpdateFinder
    {
        Task<IReadOnlyCollection<PackageUpdateSet>> FindPackageUpdateSets(
            IFolder workingFolder,
            NuGetSources sources,
            VersionChange allowedChange);
    }
}
