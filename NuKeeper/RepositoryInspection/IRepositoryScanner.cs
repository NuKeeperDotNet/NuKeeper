using System.Collections.Generic;
using NuKeeper.Files;

namespace NuKeeper.RepositoryInspection
{
    public interface IRepositoryScanner
    {
        IEnumerable<PackageInProject> FindAllNuGetPackages(IFolder workingFolder);
    }
}
