using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update.ProcessRunner;

namespace NuKeeper.Update.Process
{
    public class NuGetFileRestoreCommand : IFileRestoreCommand
    {
        private readonly NuGetSources _sources;
        private readonly INuKeeperLogger _logger;
        private readonly IExternalProcess _externalProcess;

        public NuGetFileRestoreCommand(
            INuKeeperLogger logger,
            NuGetSources sources,
            IExternalProcess externalProcess = null)
        {
            _logger = logger;
            _sources = sources;
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task Invoke(FileInfo file)
        {
            _logger.Info($"Nuget restore on {file.DirectoryName} {file.Name}");

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _logger.Info("Cannot run NuGet.exe file restore as OS Platform is not Windows");
                return;
            }

            var nuget = NuGetPath.FindExecutable();

            if (string.IsNullOrWhiteSpace(nuget))
            {
                _logger.Info("Cannot find NuGet exe for solution restore");
                return;
            }

            var sources = _sources.CommandLine("-Source");

            var arguments = $"restore {file.Name} {sources}";
            _logger.Verbose($"{nuget} {arguments}");

            var processOutput = await _externalProcess.Run(file.DirectoryName, nuget, arguments, ensureSuccess: false);

            if (processOutput.Success)
            {
                _logger.Verbose($"Nuget restore on {file.Name} complete");
            }
            else
            {
                _logger.Verbose($"Nuget restore failed on {file.DirectoryName} {file.Name}:\n{processOutput.Output}\n{processOutput.ErrorOutput}");
            }
        }

        public async Task Invoke(NuGetVersion selectedVersion, string source, PackageInProject current)
        {
            await Invoke(current.Path.Info);
        }
    }
}
