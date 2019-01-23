using System.Threading.Tasks;

namespace NuKeeper.Abstractions.ProcessRunner
{
    public interface IExternalProcess
    {
        Task<ProcessOutput> Run(string workingDirectory, string command, string arguments, bool ensureSuccess);
    }
}
