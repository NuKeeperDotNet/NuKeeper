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
            Console.WriteLine($"dotnet restore {solutionName}");
            var updateCommand = $"cd {dirName} & dotnet restore {solutionName}";
            await _externalProcess.Run(updateCommand, true);
            Console.WriteLine("restore complete");
        }
    }
}