using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Process
{
    public interface IUpdatePackageCommand
    {
        Task Invoke(NuGetVersion newVersion, PackageInProject currentPackage);
    }
}