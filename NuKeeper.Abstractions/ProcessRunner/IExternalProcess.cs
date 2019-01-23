using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.Abstractions.ProcessRunner
{
    public interface IExternalProcess
    {
        Task<ProcessOutput> Run(string workingDirectory, string command, string arguments, IEnumerable<string> input, bool ensureSuccess);
    }
}
