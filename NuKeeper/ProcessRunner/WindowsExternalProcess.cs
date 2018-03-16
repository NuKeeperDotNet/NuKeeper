using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NuKeeper.ProcessRunner
{
    public class WindowsExternalProcess : IExternalProcess
    {
        public async Task<ProcessOutput> Run(string command, bool ensureSuccess)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/C " + command)
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            var process = Process.Start(processInfo);

            var textOut = await process.StandardOutput.ReadToEndAsync();
            var errorOut = await process.StandardError.ReadToEndAsync();

            process.WaitForExit();

            var exitCode = process.ExitCode;

            if (ensureSuccess && exitCode != 0)
            {
                throw new Exception($"Exit code: {exitCode}\n\n{textOut}\n\n{errorOut}");
            }

            return new ProcessOutput(textOut, errorOut, exitCode);
        }
    }
}
