using NuKeeper.ProcessRunner;
using System;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Process
{
    public class PackagesConfigUpdater : INuGetUpdater
    {
        private readonly IExternalProcess _externalProcess;

        public PackagesConfigUpdater(IExternalProcess externalProcess = null)
        {
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task UpdatePackage(NuGetVersion newVersion, PackageInProject currentPackage)
        {
            var dirName = currentPackage.Path.FullDirectory;
            var nuget = NugetPath.Find();
            var updateCommand = $"cd {dirName}"
                + $" & {nuget} restore packages.config"
                + $" & {nuget} update packages.config -Id {currentPackage.Id} -Version {newVersion}";
            Console.WriteLine(updateCommand);

            await _externalProcess.Run(updateCommand, true);
        }
    }
}
