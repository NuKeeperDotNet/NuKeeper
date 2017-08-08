using System.Threading.Tasks;
using NuKeeper.ProcessRunner;
using NuGet.Versioning;
using NuKeeper.Logging;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Process
{
    public class NuGetUpdatePackageCommand : IUpdatePackageCommand
    {
        private readonly INuKeeperLogger _logger;
        private readonly IExternalProcess _externalProcess;

        public NuGetUpdatePackageCommand(INuKeeperLogger logger, IExternalProcess externalProcess = null)
        {
            _logger = logger;
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task Invoke(NuGetVersion newVersion, PackageInProject currentPackage)
        {
            var dirName = currentPackage.Path.FullDirectory;
            var nuget = NuGetPath.FindExecutable();
            var updateCommand = $"cd {dirName}"
                + $" & {nuget} restore packages.config"
                + $" & {nuget} update packages.config -Id {currentPackage.Id} -Version {newVersion}";
            _logger.Verbose(updateCommand);

            await _externalProcess.Run(updateCommand, true);
        }
    }
}
