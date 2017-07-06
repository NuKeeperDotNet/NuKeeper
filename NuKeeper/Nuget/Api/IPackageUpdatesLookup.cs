using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Nuget.Api
{
    public interface IPackageUpdatesLookup
    {
        Task<List<PackageUpdate>> FindUpdatesForPackages(List<NugetPackage> packages);
    }
}