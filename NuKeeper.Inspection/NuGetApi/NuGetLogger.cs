using System;
using System.Threading.Tasks;
using NuGet.Common;
using NuKeeper.Abstractions.Logging;
using LogLevel = NuGet.Common.LogLevel;

namespace NuKeeper.Inspection.NuGetApi
{
    public class NuGetLogger : LoggerBase
    {
        private readonly INuKeeperLogger _logger;

        public NuGetLogger(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public override void Log(ILogMessage message)
        {
            switch (message.Level)
            {
                case LogLevel.Verbose:
                    _logger.Detailed(message.Message);
                    break;
                case LogLevel.Debug:
                    _logger.Detailed(message.Message);
                    break;
                case LogLevel.Information:
                    _logger.Detailed(message.Message);
                    break;
                case LogLevel.Minimal:
                    _logger.Normal(message.Message);
                    break;
                case LogLevel.Warning:
                    _logger.Normal(message.Message);
                    break;
                case LogLevel.Error:
                    _logger.Error(message.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Invalid log level {message.Level}");
            }
        }

        public override Task LogAsync(ILogMessage message)
        {
            Log(message);
            return Task.CompletedTask;
        }
    }
}
