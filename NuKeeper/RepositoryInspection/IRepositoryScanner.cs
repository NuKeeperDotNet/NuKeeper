using System.Collections.Generic;

namespace NuKeeper.RepositoryInspection
{
    public interface IRepositoryScanner
    {
        IEnumerable<NugetPackage> FindAllNugetPackages(string rootDirectory);
    }
}
