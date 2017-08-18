using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.Logging;
using NuKeeper.ProcessRunner;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Process
{
    public class DotNetUpdatePackageCommand : IUpdatePackageCommand
    {
        private readonly INuKeeperLogger _logger;
        private readonly IExternalProcess _externalProcess;

        public DotNetUpdatePackageCommand(INuKeeperLogger logger, IExternalProcess externalProcess = null)
        {
            _logger = logger;
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task Invoke(NuGetVersion newVersion, string packageSource, PackageInProject currentPackage)
        {
            var dirName = currentPackage.Path.FullDirectory;
            var updateCommand = $"cd {dirName} & dotnet add package {currentPackage.Id} -v {newVersion} -s {packageSource}";
            _logger.Verbose(updateCommand);

            await _externalProcess.Run(updateCommand, true);
        }
    }
}
