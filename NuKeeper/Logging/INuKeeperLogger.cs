using System;

namespace NuKeeper.Logging
{
    public interface INuKeeperLogger
    {
        void Error(string message, Exception ex);
        void Info(string message);
        void Verbose(string message);
    }
}