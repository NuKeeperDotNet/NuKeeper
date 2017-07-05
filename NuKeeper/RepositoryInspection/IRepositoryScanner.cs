using System.Collections.Generic;
using NuKeeper.Nuget;

namespace NuKeeper.RepositoryInspection
{
    interface IRepositoryScanner
    {
        IEnumerable<NugetPackage> FindAllNugetPackages(string rootDirectory);
    }
}
