using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.ProcessRunner;
using NuKeeper.Types.Formats;
using NuKeeper.Types.Logging;

namespace NuKeeper.NuGet.Process
{
    public class NuGetFileRestoreCommand : IFileRestoreCommand
    {
        private readonly string[] _sources;
        private readonly INuKeeperLogger _logger;
        private readonly IExternalProcess _externalProcess;

        public NuGetFileRestoreCommand(
            INuKeeperLogger logger,
            UserSettings settings,
            IExternalProcess externalProcess = null)
        {
            _logger = logger;
            _sources = settings.NuGetSources;
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task Invoke(FileInfo file)
        {
            _logger.Info($"Nuget restore on {file.DirectoryName} {file.Name}");
            var nuget = NuGetPath.FindExecutable();
            var sources = GetSourcesCommandLine(_sources);

            var arguments = $"restore {file.Name} {sources}";
            _logger.Verbose(arguments);

            var processOutput = await _externalProcess.Run(file.DirectoryName, nuget, arguments, ensureSuccess: false);

            if (processOutput.Success)
            {
                _logger.Verbose($"Nuget restore on {file.Name} complete");
            }
            else
            {
                _logger.Verbose($"Restore failed on {file.DirectoryName} {file.Name}:\n{processOutput.Output}\n{processOutput.ErrorOutput}");
            }
        }

        private static string GetSourcesCommandLine(IEnumerable<string> sources)
        {
            return sources.Select(s => $"-Source {s}").JoinWithSeparator(" ");
        }
    }
}
