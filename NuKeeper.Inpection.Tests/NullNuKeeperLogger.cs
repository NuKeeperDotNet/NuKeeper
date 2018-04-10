using System;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Inpection.Tests
{
    internal class NullNuKeeperLogger : INuKeeperLogger
    {
        public void Error(string message, Exception ex)
        {
        }

        public void Terse(string message)
        {
        }

        public void Info(string message)
        {
        }

        public void Verbose(string message)
        {
        }
    }
}
