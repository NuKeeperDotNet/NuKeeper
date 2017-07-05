using System;
using NuGet.Common;

namespace NuKeeper.Nuget
{
    public class NugetLogger : ILogger
    {
        public void LogDebug(string data) => WriteLine($"DEBUG: {data}");
        public void LogVerbose(string data) => WriteLine($"VERBOSE: {data}");
        public void LogInformation(string data) => WriteLine($"INFORMATION: {data}");
        public void LogMinimal(string data) => WriteLine($"MINIMAL: {data}");
        public void LogWarning(string data) => WriteLine($"WARNING: {data}");
        public void LogError(string data) => WriteLine($"ERROR: {data}");
        public void LogInformationSummary(string data) => WriteLine($"InfoSummary: {data}");
        public void LogErrorSummary(string data) => WriteLine($"ErrorSummary: {data}");

        private void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}
