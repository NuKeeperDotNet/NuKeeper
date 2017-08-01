using System;
using System.IO;
using System.Threading.Tasks;
using NuKeeper.ProcessRunner;

namespace NuKeeper.NuGet.Process
{
    public class DotNetRestoreCommand: ISolutionRestoreCommand
    {
        private readonly IExternalProcess _externalProcess;

        public DotNetRestoreCommand(IExternalProcess externalProcess = null)
        {
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task Restore(string solutionPath)
        {
            var dirName = Path.GetDirectoryName(solutionPath);
            var solutionName = Path.GetFileName(solutionPath);

            Console.WriteLine($"dotnet restore {solutionName}");
            var updateCommand = $"cd {dirName} & dotnet restore {solutionName}";
            await _externalProcess.Run(updateCommand, true);
            Console.WriteLine("restore complete");
        }
    }
}