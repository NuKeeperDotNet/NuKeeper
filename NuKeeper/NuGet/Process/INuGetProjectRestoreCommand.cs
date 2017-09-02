using System.Threading.Tasks;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Process
{
    public interface INuGetProjectRestoreCommand
    {
        Task Invoke(PackageInProject currentPackage);
    }
}