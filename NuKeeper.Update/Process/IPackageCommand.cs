using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Versioning;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Update.Process
{
    public interface IPackageCommand
    {
        Task Invoke(PackageInProject currentPackage,
            NuGetVersion newVersion, PackageSource packageSource, NuGetSources allSources);
    }
}
