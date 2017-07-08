using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Api
{
    public interface IPackageUpdatesLookup
    {
        Task<List<PackageUpdate>> FindUpdatesForPackages(List<NuGetPackage> packages);
    }
}