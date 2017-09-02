using System.IO;
using System.Threading.Tasks;
using NuKeeper.Logging;
using NuKeeper.ProcessRunner;

namespace NuKeeper.NuGet.Process
{
    public class NuGetSolutionRestoreCommand: ISolutionRestoreCommand
    {
        private readonly INuKeeperLogger _logger;
        private readonly IExternalProcess _externalProcess;

        public NuGetSolutionRestoreCommand(INuKeeperLogger logger, IExternalProcess externalProcess = null)
        {
            _logger = logger;
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task Restore(FileInfo solutionFile)
        {
            var nuget = NuGetPath.FindExecutable();

            _logger.Info($"Solutions nuget restore on {solutionFile.DirectoryName} {solutionFile.Name}");

            var updateCommand = $"cd {solutionFile.DirectoryName} & {nuget} restore {solutionFile.Name}";
            _logger.Verbose(updateCommand);

            var processOutput = await _externalProcess.Run(updateCommand, ensureSuccess: false);

            if (processOutput.Success)
            {
                _logger.Verbose("Solution restore complete");
            }
            else
            {
                _logger.Verbose($"Solution restore failed on {solutionFile.DirectoryName} {solutionFile.Name}:\n{processOutput.Output}\n{processOutput.ErrorOutput}");}
        }
    }
}
