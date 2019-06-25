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

        public async Task<bool> HasMono()
        {
            return await _checkMono;
        }

        public async Task<ProcessOutput> Run(string workingDirectory, string command, string arguments, bool ensureSuccess)
        {
            _logger.Normal($"Using mono to run '{command}'");

            if (!await HasMono())
            {
                _logger.Error($"Cannot run '{command}' on mono since mono was not found");
                throw new InvalidOperationException("Mono was not found");
            }

            var process = new ExternalProcess(_logger);
            var processOutput = await process.Run(workingDirectory, "mono", $"{command} {arguments}", false);

            return processOutput;
        }

        private async Task<bool> CheckMonoExists()
        {
            var process = new ExternalProcess(_logger);
            var result = await process.Run("", "mono", "--version", false);

            return result.Success;
        }
    }
}
