using NuGet.Common;

namespace NuKeeper.Integration.Tests.NuGet.Api
{
    internal class NullNuGetLogger : ILogger
    {
        public void LogDebug(string data)
        {
        }

        public void LogVerbose(string data)
        {
        }

        public void LogInformation(string data)
        {
        }

        public void LogMinimal(string data)
        {
        }

        public void LogWarning(string data)
        {
        }

        public void LogError(string data)
        {
        }

        public void LogInformationSummary(string data)
        {
        }

        public void LogErrorSummary(string data)
        {
        }
    }
}