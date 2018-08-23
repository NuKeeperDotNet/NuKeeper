using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Versioning;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update.ProcessRunner;

namespace NuKeeper.Update.Process
{
    public class NuGetFileRestoreCommand : IFileRestoreCommand
    {
        private readonly INuKeeperLogger _logger;
        private readonly INuGetPath _nuGetPath;
        private readonly IExternalProcess _externalProcess;

        public NuGetFileRestoreCommand(
            INuKeeperLogger logger,
            INuGetPath nuGetPath,
            IExternalProcess externalProcess)
        {
            _logger = logger;
            _nuGetPath = nuGetPath;
            _externalProcess = externalProcess;
        }

        public async Task Invoke(FileInfo file, NuGetSources sources)
        {
            _logger.Normal($"Nuget restore on {file.DirectoryName} {file.Name}");

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _logger.Normal("Cannot run NuGet.exe file restore as OS Platform is not Windows");
                return;
            }

            var nuget = _nuGetPath.FindExecutable();

            if (string.IsNullOrWhiteSpace(nuget))
            {
                _logger.Normal("Cannot find NuGet exe for solution restore");
                return;
            }

            var sourcesCommandLine = sources.CommandLine("-Source");

            var restoreCommand = $"restore {file.Name} {sourcesCommandLine}";

            var processOutput = await _externalProcess.Run(file.DirectoryName, nuget, restoreCommand, ensureSuccess: false);

            if (processOutput.Success)
            {
                _logger.Detailed($"Nuget restore on {file.Name} complete");
            }
            else
            {
                _logger.Detailed($"Nuget restore failed on {file.DirectoryName} {file.Name}:\n{processOutput.Output}\n{processOutput.ErrorOutput}");
            }
        }

        public async Task Invoke(PackageInProject currentPackage,
            NuGetVersion newVersion, PackageSource packageSource, NuGetSources allSources)
        {
            await Invoke(currentPackage.Path.Info, allSources);
        }
    }
}
