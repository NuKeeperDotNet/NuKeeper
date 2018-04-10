using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection
{
    public interface IUpdateFinder
    {
        Task<List<PackageUpdateSet>> FindPackageUpdateSets(IFolder workingFolder);
    }
}
