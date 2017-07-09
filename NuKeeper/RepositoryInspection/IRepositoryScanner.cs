using System.Collections.Generic;

namespace NuKeeper.RepositoryInspection
{
    public interface IRepositoryScanner
    {
        IEnumerable<PackageInProject> FindAllNuGetPackages(string rootDirectory);
    }
}
