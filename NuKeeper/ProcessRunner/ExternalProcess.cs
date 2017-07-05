using System.Diagnostics;
using System.Threading.Tasks;

namespace NuKeeper.ProcessRunner
{
    public class ExternalProcess : IExternalProcess
    {
        public async Task<ProcessOutput> Run(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/C " + command)
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            var process = Process.Start(processInfo);

            var textOut = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();

            var exitCode = process.ExitCode;

            return new ProcessOutput(textOut, exitCode);
        }
    }
}