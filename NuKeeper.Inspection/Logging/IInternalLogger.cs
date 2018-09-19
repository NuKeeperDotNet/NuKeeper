using System;

namespace NuKeeper.Inspection.Logging
{
    public interface IInternalLogger
    {
        void Error(string message, Exception ex = null);
        void Log(LogLevel level, string message);
    }
}
