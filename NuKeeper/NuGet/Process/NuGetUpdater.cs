using System;
using System.Threading.Tasks;
using NuGet.Versioning;
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

        public async Task UpdatePackage(NuGetVersion newVersion, PackageInProject currentPackage)
        {
            var dirName = currentPackage.Path.FullDirectory;
            var updateCommand = $"cd {dirName} & dotnet add package {currentPackage.Id} -v {newVersion}";
            Console.WriteLine(updateCommand);

            await _externalProcess.Run(updateCommand, true);
        }
    }
}
