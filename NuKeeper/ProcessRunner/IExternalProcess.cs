using System.Threading.Tasks;

namespace NuKeeper.ProcessRunner
{
    public interface IExternalProcess
    {
        Task<ProcessOutput> Run(string workingDirectory, string command, string arguments, bool ensureSuccess);
    }
}
