using System.Threading.Tasks;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Update.Process
{
    public interface IDotNetRestoreCommand : IPackageCommand
    {
        Task Invoke(PackageInProject currentPackage, NuGetSources allSources);
    }
}
