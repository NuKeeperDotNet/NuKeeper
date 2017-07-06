using System;
using System.Threading.Tasks;
using NuKeeper.ProcessRunner;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Nuget.Process
{
    public class NugetUpdate : INugetUpdate
    {
        private readonly IExternalProcess _externalProcess;

        public NugetUpdate(IExternalProcess externalProcess)
        {
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task UpdatePackage(NugetPackage package, string pathToSolutionFile)
        {
            var updateCommand = $"nuget update {pathToSolutionFile} -Id {package.Id} -Version {package.Version} - NonInteractive";
            await RunExternalCommand(updateCommand);
        }

        private async Task RunExternalCommand(string command)
        {
            var result = await _externalProcess.Run($"{command}");

            if (!result.Success)
            {
                throw new Exception($"Exit code: {result.ExitCode}\n\n{result.Output}");
            }
        }
    }
}
