using System;

namespace NuKeeper.Inspection.Logging
{
    public class ConsoleLogger : INuKeeperLogger, IConfigureLogLevel
    {
        private LogLevel _logLevel = LogLevel.Normal;

        public void Error(string message, Exception ex = null)
        {
            if (ex == null)
            {
                Console.Error.WriteLine(message);
            }
            else
            {
                Console.Error.WriteLine($"{message} {ex.GetType().Name} : {ex.Message}");
                if (_logLevel == LogLevel.Detailed)
                {
                    Console.Error.WriteLine(ex.StackTrace);
                }
            }
        }

        public void Terse(string message)
        {
            LogWithLevel(message, LogLevel.Minimal);
        }

        public void Info(string message)
        {
            LogWithLevel(message, LogLevel.Normal);
        }

        public void Verbose(string message)
        {
            LogWithLevel(message, LogLevel.Detailed);
        }

        private void LogWithLevel(string message, LogLevel level)
        {
            if (_logLevel >= level)
            {
                Console.WriteLine(message);
            }
        }

        void IConfigureLogLevel.SetLogLevel(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }
    }
}
