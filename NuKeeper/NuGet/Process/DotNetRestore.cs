using System;
using System.Threading.Tasks;
using NuKeeper.ProcessRunner;

namespace NuKeeper.NuGet.Process
{
    public class DotNetRestore: ISolutionRestore
    {
        private readonly IExternalProcess _externalProcess;

        public DotNetRestore(IExternalProcess externalProcess = null)
        {
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task Restore(string dirName, string solutionName)
        {
            var updateCommand = $"cd {dirName} & dotnet restore {solutionName}";
            await RunExternalCommand(updateCommand);
        }

        private async Task RunExternalCommand(string command)
        {
            var result = await _externalProcess.Run(command);

            if (!result.Success)
            {
                throw new Exception($"Exit code: {result.ExitCode}\n\n{result.Output}\n\n{result.ErrorOutput}");
            }
        }

    }
}