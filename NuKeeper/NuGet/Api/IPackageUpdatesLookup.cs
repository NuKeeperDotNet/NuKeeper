using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Nuget.Api;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Api
{
    public interface IPackageUpdatesLookup
    {
        Task<List<PackageUpdateSet>> FindUpdatesForPackages(List<PackageInProject> packages);
    }
}