using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update.ProcessRunner;

namespace NuKeeper.Update.Process
{
    public class NuGetUpdatePackageCommand : IPackageCommand
    {
        private readonly IExternalProcess _externalProcess;
        private readonly INuKeeperLogger _logger;
        private readonly NuGetSources _sources;

        public NuGetUpdatePackageCommand(
            INuKeeperLogger logger,
            NuGetSources sources,
            IExternalProcess externalProcess = null)
        {
            _logger = logger;
            _sources = sources;
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task Invoke(NuGetVersion newVersion, string packageSource, PackageInProject currentPackage)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _logger.Info("Cannot run NuGet.exe package update as OS Platform is not Windows");
                return;
            }

            var dirName = currentPackage.Path.Info.DirectoryName;

            var nuget = NuGetPath.FindExecutable();
            if (string.IsNullOrWhiteSpace(nuget))
            {
                _logger.Info("Cannot find NuGet exe for package update");
                return;
            }

            var sources = _sources.CommandLine("-Source");
            var arguments = $"update packages.config -Id {currentPackage.Id} -Version {newVersion} {sources}";
            _logger.Verbose(arguments);

            await _externalProcess.Run(dirName, nuget, arguments, true);
        }
    }
}
