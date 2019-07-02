using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Versioning;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Update.ProcessRunner;

namespace NuKeeper.Update.Process
{
    public class NuGetFileRestoreCommand : IFileRestoreCommand
    {
        private readonly INuKeeperLogger _logger;
        private readonly INuGetPath _nuGetPath;
        private readonly IMonoExecutor _monoExecutor;
        private readonly IExternalProcess _externalProcess;

        public NuGetFileRestoreCommand(
            INuKeeperLogger logger,
            INuGetPath nuGetPath,
            IMonoExecutor monoExecutor,
            IExternalProcess externalProcess)
        {
            _logger = logger;
            _nuGetPath = nuGetPath;
            _monoExecutor = monoExecutor;
            _externalProcess = externalProcess;
        }

        public async Task Invoke(FileInfo file, NuGetSources sources)
        {
            _logger.Normal($"Nuget restore on {file.DirectoryName} {file.Name}");

            var nuget = _nuGetPath.Executable;

            if (string.IsNullOrWhiteSpace(nuget))
            {
                _logger.Normal("Cannot find NuGet.exe for solution restore");
                return;
            }

            var sourcesCommandLine = sources.CommandLine("-Source");

            var restoreCommand = $"restore {file.Name} {sourcesCommandLine}  -NonInteractive";

            ProcessOutput processOutput;
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (await _monoExecutor.CanRun())
                {
                    processOutput = await _monoExecutor.Run(file.DirectoryName,
                        nuget,
                        restoreCommand,
                        ensureSuccess: false);
                }
                else
                {
                    _logger.Error("Cannot run NuGet.exe. It requires either Windows OS Platform or Mono installation");
                    return;
                }
            }
            else
            {
                processOutput = await _externalProcess.Run(file.DirectoryName,
                    nuget,
                    restoreCommand,
                    ensureSuccess: false);
            }

            if (processOutput.Success)
            {
                _logger.Detailed($"Nuget restore on {file.Name} complete");
            }
            else
            {
                _logger.Detailed(
                    $"Nuget restore failed on {file.DirectoryName} {file.Name}:\n{processOutput.Output}\n{processOutput.ErrorOutput}");
            }
        }

        public async Task Invoke(PackageInProject currentPackage,
            NuGetVersion newVersion, PackageSource packageSource, NuGetSources allSources)
        {
            await Invoke(currentPackage.Path.Info, allSources);
        }
    }
}
