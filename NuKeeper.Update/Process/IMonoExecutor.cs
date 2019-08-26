using System.Threading.Tasks;
using NuKeeper.Update.ProcessRunner;

namespace NuKeeper.Update.Process
{
    public interface IMonoExecutor : IExternalProcess
    {
        Task<bool> CanRun();
    }
}
