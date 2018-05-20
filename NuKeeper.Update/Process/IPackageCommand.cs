using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Update.Process
{
    public interface IPackageCommand
    {
        Task Invoke(NuGetVersion newVersion, string packageSource, PackageInProject currentPackage);
    }
}
