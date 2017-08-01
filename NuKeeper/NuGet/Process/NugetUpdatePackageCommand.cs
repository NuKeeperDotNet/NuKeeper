using NuKeeper.ProcessRunner;
using System;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Process
{
    public class NugetUpdatePackageCommand : IUpdatePackageCommand
    {
        private readonly IExternalProcess _externalProcess;

        public NugetUpdatePackageCommand(IExternalProcess externalProcess = null)
        {
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task Invoke(NuGetVersion newVersion, PackageInProject currentPackage)
        {
            var dirName = currentPackage.Path.FullDirectory;
            var nuget = NuGetPath.FindExecutable();
            var updateCommand = $"cd {dirName}"
                + $" & {nuget} restore packages.config"
                + $" & {nuget} update packages.config -Id {currentPackage.Id} -Version {newVersion}";
            Console.WriteLine(updateCommand);

            await _externalProcess.Run(updateCommand, true);
        }
    }
}
