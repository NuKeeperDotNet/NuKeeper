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
            await _externalProcess.Run(updateCommand, true);
            Console.WriteLine("restore complete");
        }
    }
}