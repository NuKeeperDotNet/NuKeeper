using System;
using System.IO;
using System.Threading.Tasks;
using NuKeeper.NuGet.Api;
using NuKeeper.ProcessRunner;

namespace NuKeeper.NuGet.Process
{
    public class NuGetUpdater : INuGetUpdater
    {
        private readonly IExternalProcess _externalProcess;

        public NuGetUpdater(IExternalProcess externalProcess = null)
        {
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task UpdatePackage(PackageUpdate update)
        {
            var dirName = Path.GetDirectoryName(update.CurrentPackage.SourceFilePath);
            var updateCommand = $"cd {dirName} & dotnet add package {update.PackageId} -v {update.NewVersion}";
            Console.WriteLine(updateCommand);
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
