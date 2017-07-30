using System.Threading.Tasks;

namespace NuKeeper.NuGet.Process
{
    public interface ISolutionRestore
    {
        Task Restore(string dirName, string solutionName);
    }
}