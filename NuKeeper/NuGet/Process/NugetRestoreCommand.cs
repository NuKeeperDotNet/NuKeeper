using System;
using System.IO;
using System.Threading.Tasks;
using NuKeeper.ProcessRunner;

namespace NuKeeper.NuGet.Process
{
    public class NugetRestoreCommand: ISolutionRestoreCommand
    {
        private readonly IExternalProcess _externalProcess;

        public NugetRestoreCommand(IExternalProcess externalProcess = null)
        {
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task Restore(string solutionPath)
        {
            var dirName = Path.GetDirectoryName(solutionPath);
            var solutionName = Path.GetFileName(solutionPath);
            Console.WriteLine($"nuget restore {solutionName}");

            var nuget = NugetPath.Find();
            var updateCommand = $"cd {dirName} & {nuget} restore {solutionName}";
            await _externalProcess.Run(updateCommand, true);
            Console.WriteLine("restore complete");
        }
    }
}