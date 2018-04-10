using System.Collections.Generic;
using NuKeeper.Inspection.Files;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public interface IRepositoryScanner
    {
        IEnumerable<PackageInProject> FindAllNuGetPackages(IFolder workingFolder);
    }
}
