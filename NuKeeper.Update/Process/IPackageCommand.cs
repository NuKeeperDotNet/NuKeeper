using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Update.Process
{
    public interface IPackageCommand
    {
        Task Invoke(PackageInProject currentPackage,
            NuGetVersion newVersion, string packageSource, NuGetSources allSources);
    }
}
