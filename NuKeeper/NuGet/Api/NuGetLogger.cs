using NuGet.Common;

namespace NuKeeper.NuGet.Api
{
    public class NuGetLogger : ILogger
    {
        private readonly Logging.INuKeeperLogger _logger;

        public NuGetLogger(Logging.INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public void LogDebug(string data) => _logger.Verbose(data);
        public void LogVerbose(string data) => _logger.Verbose(data);
        public void LogInformation(string data) => _logger.Verbose(data);
        public void LogMinimal(string data) => _logger.Info(data);
        public void LogWarning(string data) => _logger.Info(data);
        public void LogError(string data) => _logger.Info(data);
        public void LogInformationSummary(string data) => _logger.Info(data);
        public void LogErrorSummary(string data) => _logger.Info(data);
    }
}
