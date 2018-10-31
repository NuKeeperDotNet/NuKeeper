using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.RepositoryInspection;

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
