using System;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Logging
{
    public class ConsoleLogger : INuKeeperLogger
    {
        private readonly LogLevel _logLevel;

        public ConsoleLogger(UserSettings settings)
        {
            _logLevel = settings.LogLevel;
        }

        public void Error(string message, Exception ex = null)
        {
            if (ex == null)
            {
                Console.Error.WriteLine(message);
            }
            else
            {
                Console.Error.WriteLine($"{message} {ex.GetType().Name} : {ex.Message}");
            }
        }

        public void Terse(string message)
        {
            LogWithLevel(message, LogLevel.Terse);
        }

        public void Info(string message)
        {
            LogWithLevel(message, LogLevel.Info);
        }

        public void Verbose(string message)
        {
            LogWithLevel(message, LogLevel.Verbose);
        }

        private void LogWithLevel(string message, LogLevel level)
        {
            if (_logLevel >= level)
            {
                Console.WriteLine(message);
            }
        }
    }
}
