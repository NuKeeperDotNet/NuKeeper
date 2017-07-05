using Greenkeeper.Nuget;
using System.Collections.Generic;

namespace Greenkeeper.RepositoryInspection
{
    interface IRepositoryScanner
    {
        IEnumerable<NugetPackage> FindAllNugetPackages(string rootDirectory);
    }
}
