using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Types.Files;

namespace NuKeeper.Inspection
{
    public interface IUpdateFinder
    {
        Task<List<PackageUpdateSet>> FindPackageUpdateSets(IFolder workingFolder);
    }
}
