using System;
using System.Threading.Tasks;
using NuKeeper.ProcessRunner;

namespace NuKeeper.NuGet.Process
{
    public class NugetRestore: ISolutionRestore
    {
        private readonly IExternalProcess _externalProcess;

        public NugetRestore(IExternalProcess externalProcess = null)
        {
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task Restore(string dirName, string solutionName)
        {
            Console.WriteLine($"nuget restore {solutionName}");

            var nuget = NugetPath.Find();
            var updateCommand = $"cd {dirName} & {nuget} restore {solutionName}";
            await RunExternalCommand(updateCommand);
            Console.WriteLine("restore complete");
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