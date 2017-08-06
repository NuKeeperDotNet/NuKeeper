using System;
using NuKeeper.Logging;

namespace NuKeeper.Integration.Tests.Nuget.Api
{
    public class NullNuKeeperLogger : INuKeeperLogger
    {
        public void Error(string message, Exception ex)
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