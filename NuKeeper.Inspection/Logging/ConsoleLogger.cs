using System;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.Inspection.Logging
{
    public class ConsoleLogger : IInternalLogger
    {
        private readonly LogLevel _logLevel;

        public ConsoleLogger(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }

        public void LogError(string message, Exception ex)
        {
            var saveColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;

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
            finally
            {
                Console.ForegroundColor = saveColor;
            }
        }

        public void Log(LogLevel level, string message)
        {
            if (_logLevel >= level)
            {
                Console.WriteLine(message);
            }
        }
    }
}
