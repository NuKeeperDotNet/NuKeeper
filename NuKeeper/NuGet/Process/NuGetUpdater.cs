using System;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuKeeper.ProcessRunner;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Process
{
    public class NuGetUpdater : INuGetUpdater
    {
        private readonly IExternalProcess _externalProcess;

        public NuGetUpdater(IExternalProcess externalProcess = null)
        {
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task UpdatePackage(PackageIdentity newPackage, PackageInProject currentPackage)
        {
            var dirName = currentPackage.Path.FullDirectory;
            var updateCommand = $"cd {dirName} & dotnet add package {newPackage.Id} -v {newPackage.Version}";
            Console.WriteLine(updateCommand);
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
