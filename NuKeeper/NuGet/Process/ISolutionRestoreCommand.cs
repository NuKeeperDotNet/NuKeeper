using System.Threading.Tasks;

namespace NuKeeper.NuGet.Process
{
    public interface ISolutionRestoreCommand
    {
        Task Restore(string solutionPath);
    }
}