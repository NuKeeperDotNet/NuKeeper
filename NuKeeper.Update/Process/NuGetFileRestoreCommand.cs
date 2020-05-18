using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
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
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            var fileNameArg = ArgumentEscaper.EscapeAndConcatenate(new[] { file.Name });
            _logger.Normal($"Nuget restore on {file.DirectoryName} {fileNameArg}");

            var nuget = _nuGetPath.Executable;

            if (string.IsNullOrWhiteSpace(nuget))
            {
                _logger.Normal("Cannot find NuGet.exe for solution restore");
                return;
            }

            var sourcesCommandLine = sources.CommandLine("-Source");

            var restoreCommand = $"restore {fileNameArg} {sourcesCommandLine}  -NonInteractive";

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
                _logger.Detailed($"Nuget restore on {fileNameArg} complete");
            }
            else
            {
                _logger.Detailed(
                    $"Nuget restore failed on {file.DirectoryName} {fileNameArg}:\n{processOutput.Output}\n{processOutput.ErrorOutput}");
            }
        }

        public async Task Invoke(PackageInProject currentPackage,
            NuGetVersion newVersion, PackageSource packageSource, NuGetSources allSources)
        {
            if (currentPackage == null)
            {
                throw new ArgumentNullException(nameof(currentPackage));
            }

            await Invoke(currentPackage.Path.Info, allSources);
        }
    }
}
