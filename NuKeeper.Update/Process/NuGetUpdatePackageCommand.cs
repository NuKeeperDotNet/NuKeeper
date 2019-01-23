using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Versioning;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.ProcessRunner;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Update.Process
{
    public class NuGetUpdatePackageCommand : INuGetUpdatePackageCommand
    {
        private readonly IExternalProcess _externalProcess;
        private readonly INuKeeperLogger _logger;
        private readonly INuGetPath _nuGetPath;

        public NuGetUpdatePackageCommand(
            INuKeeperLogger logger,
            INuGetPath nuGetPath,
            IExternalProcess externalProcess)
        {
            _logger = logger;
            _nuGetPath = nuGetPath;
            _externalProcess = externalProcess;
        }

        public async Task Invoke(PackageInProject currentPackage,
            NuGetVersion newVersion, PackageSource packageSource, NuGetSources allSources)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _logger.Normal("Cannot run NuGet.exe package update as OS Platform is not Windows");
                return;
            }

            var projectPath = currentPackage.Path.Info.DirectoryName;

            var nuget = _nuGetPath.Executable;
            if (string.IsNullOrWhiteSpace(nuget))
            {
                _logger.Normal("Cannot find NuGet exe for package update");
                return;
            }

            var sources = allSources.CommandLine("-Source");
            var updateCommand = $"update packages.config -Id {currentPackage.Id} -Version {newVersion} {sources} -NonInteractive";
            await _externalProcess.Run(projectPath, nuget, updateCommand, null, true);
        }
    }
}
