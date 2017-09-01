using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Process
{
    public interface INuGetProjectRestoreCommand
    {
        Task Invoke(NuGetVersion newVersion, string packageSource, PackageInProject currentPackage);
    }
}