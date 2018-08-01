using System;

namespace NuKeeper.Inspection.Logging
{
    public interface INuKeeperLogger
    {
        void Error(string message, Exception ex = null);
        void Minimal(string message);
        void Normal(string message);
        void Detailed(string message);
    }
}
