using System.Threading.Tasks;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Nuget
{
    public interface INugetUpdate
    {
        Task UpdatePackage(NugetPackage package, string pathToSolutionFile);
    }
}