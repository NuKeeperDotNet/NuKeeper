using System;
using System.Threading.Tasks;
using NuGet.Common;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Update.ProcessRunner;

namespace NuKeeper.Update.Process
{
    public class MonoExecutor : IMonoExecutor
    {
        private readonly INuKeeperLogger _logger;
        private readonly IExternalProcess _externalProcess;

        private readonly AsyncLazy<bool> _checkMono;

        public MonoExecutor(INuKeeperLogger logger, IExternalProcess externalProcess)
        {
            _logger = logger;
            _externalProcess = externalProcess;
            _checkMono = new AsyncLazy<bool>(CheckMonoExists);
        }

        public async Task<bool> CanRun()
        {
            return await _checkMono;
        }

        public async Task<ProcessOutput> Run(string workingDirectory, string command, string arguments, bool ensureSuccess)
        {
            _logger.Normal($"Using Mono to run '{command}'");

            if (!await CanRun())
            {
                _logger.Error($"Cannot run '{command}' on Mono since Mono installation was not found");
                throw new InvalidOperationException("Mono installation was not found");
            }

            return await _externalProcess.Run(workingDirectory, "mono", $"{command} {arguments}", ensureSuccess);
        }

        private async Task<bool> CheckMonoExists()
        {
            var result = await _externalProcess.Run("", "mono", "--version", false);

            return result.Success;
        }
    }
}
