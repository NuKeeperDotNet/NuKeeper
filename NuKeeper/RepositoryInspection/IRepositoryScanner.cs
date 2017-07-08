using System.Collections.Generic;

namespace NuKeeper.RepositoryInspection
{
    public interface IRepositoryScanner
    {
        IEnumerable<NuGetPackage> FindAllNuGetPackages(string rootDirectory);
    }
}
