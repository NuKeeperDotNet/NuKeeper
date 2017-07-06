using System;
using System.Threading.Tasks;
using NuKeeper.ProcessRunner;

namespace NuKeeper.Git
{
    class Git : IGit
    {
        private readonly IExternalProcess _externalProcess;
        private readonly string _tempDirectory;

        public Git(string repoStoragePath, IExternalProcess externalProcess = null)
        {
            _externalProcess = externalProcess ?? new ExternalProcess();
            _tempDirectory = repoStoragePath;
        }

        public async Task Clone(Uri pullEndpoint)
        {
            await RunExternalCommand($"git clone {pullEndpoint} {_tempDirectory} --depth 1");
        }

        public async Task Checkout(string branchName)
        {
            await RunExternalCommand($"cd {_tempDirectory} & git checkout -b {branchName}");
        }

        public async Task Commit(string message)
        {
            await RunExternalCommand($"cd {_tempDirectory} & git commit -a -m \"{message}\"");
        }

        public async Task Push(string remoteName, string branchName)
        {
            await RunExternalCommand($"cd {_tempDirectory} & git push {remoteName} {branchName}");
        }

        private async Task RunExternalCommand(string command)
        {
            Console.WriteLine("git command: " + command);
            var result = await _externalProcess.Run($"{command}");

            if (!result.Success)
            {
                throw new Exception($"Exit code: {result.ExitCode}\n\n{result.Output}");
            }
        }
    }
}