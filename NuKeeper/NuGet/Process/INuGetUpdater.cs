using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Process
{
    public interface INuGetUpdater
    {
        Task UpdatePackage(NuGetVersion newVersion, PackageInProject currentPackage);
    }
}