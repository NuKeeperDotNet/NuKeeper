using System;

namespace NuKeeper.Inspection.Logging
{
    public interface IInternalLogger
    {
        void LogError(string message, Exception ex);
        void Log(LogLevel level, string message);
    }
}
