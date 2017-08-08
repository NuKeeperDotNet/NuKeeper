using System;
using NuKeeper.Configuration;

namespace NuKeeper.Logging
{
    public class ConsoleLogger : INuKeeperLogger
    {
        private readonly LogLevel _logLevel;

        public ConsoleLogger(Settings settings)
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
            LogWithlevel(message, LogLevel.Terse);
        }

        public void Info(string message)
        {
            LogWithlevel(message, LogLevel.Info);
        }

        public void Verbose(string message)
        {
            LogWithlevel(message, LogLevel.Verbose);
        }

        private void LogWithlevel(string message, LogLevel level)
        {
            if (_logLevel >= level)
            {
                Console.WriteLine(message);
            }
        }
    }
}