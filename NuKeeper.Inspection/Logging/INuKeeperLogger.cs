using System;

namespace NuKeeper.Inspection.Logging
{
    public interface INuKeeperLogger
    {
        void Error(string message, Exception ex = null);
        void Terse(string message);
        void Info(string message);
        void Verbose(string message);
    }
}
