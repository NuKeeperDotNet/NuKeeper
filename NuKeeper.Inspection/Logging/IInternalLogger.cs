using System;

namespace NuKeeper.Inspection.Logging
{
    public interface IInternalLogger
    {
        void Error(string message, Exception ex);
        void Log(LogLevel level, string message);
    }
}
