using System.Threading.Tasks;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Nuget.Process
{
    public interface INugetUpdate
    {
        Task UpdatePackage(NugetPackage package);
    }
}