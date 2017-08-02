using System.Threading.Tasks;

namespace NuKeeper.ProcessRunner
{
    public interface IExternalProcess
    {
        Task<ProcessOutput> Run(string command, bool ensureSuccess);
    }
}