using System.Collections.Generic;
using NuKeeper.Nuget;

namespace NuKeeper.RepositoryInspection
{
    public interface IRepositoryScanner
    {
        IEnumerable<NugetPackage> FindAllNugetPackages(string rootDirectory);
    }
}
