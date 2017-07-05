using System.Threading.Tasks;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Nuget
{

    public class NugetUpdate
    {
        public Task UpdatePackage(NugetPackage package, string pathToSolutionFile)
        {
            return Task.CompletedTask;
        }
    }
}
