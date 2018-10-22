using System;

namespace NuKeeper.Inspection.Logging
{
    public class NullLogger : IInternalLogger
    {
        public void Log(LogLevel level, string message)
        {
        }

        public void LogError(string message, Exception ex)
        {
        }
    }
}
