using System;

namespace NuKeeper.Logging
{
    public interface INuKeeperLogger
    {
        void Error(string message, Exception ex = null);
        void Summary(string message);
        void Info(string message);
        void Verbose(string message);
    }
}