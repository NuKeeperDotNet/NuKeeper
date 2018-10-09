using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Update.ProcessRunner
{
    public class ExternalProcess : IExternalProcess
    {
        private readonly INuKeeperLogger _logger;

        public ExternalProcess(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public async Task<ProcessOutput> Run(string workingDirectory, string command, string arguments, bool ensureSuccess)
        {
            _logger.Detailed($"In path {workingDirectory}, running command: {command} {arguments}");

            System.Diagnostics.Process process;

            try
            {
                var processInfo = MakeProcessStartInfo(workingDirectory, command, arguments);
                process = System.Diagnostics.Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                _logger.Error($"External command failed:{command} {arguments}", ex);

                if (ensureSuccess)
                {
                    throw;
                }

                var message = $"LogError starting external process for {command}: {ex.GetType().Name} {ex.Message}";
                return new ProcessOutput(string.Empty, message, 1);
            }

            if (process == null)
            {
                throw new NuKeeperException($"Could not start external process for {command}");
            }

            var textOut = await process.StandardOutput.ReadToEndAsync()
                .ConfigureAwait(false);
            var errorOut = await process.StandardError.ReadToEndAsync()
                .ConfigureAwait(false);

            process.WaitForExit();

            var exitCode = process.ExitCode;

            if (exitCode != 0)
            {
                var message = $"Command {command} failed with exit code: {exitCode}\n\n{textOut}\n\n{errorOut}";
                _logger.Detailed(message);

                if (ensureSuccess)
                {
                    throw new NuKeeperException(message);
                }
            }

            return new ProcessOutput(textOut, errorOut, exitCode);
        }

        private static ProcessStartInfo MakeProcessStartInfo(string workingDirectory, string command, string arguments)
        {
            return new ProcessStartInfo(command, arguments)
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = workingDirectory
            };
        }
    }
}
