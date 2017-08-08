using System;
using NuKeeper.Logging;

namespace NuKeeper.Integration.Tests.Nuget.Api
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