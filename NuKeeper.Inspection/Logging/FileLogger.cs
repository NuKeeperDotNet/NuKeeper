using System;
using System.IO;

namespace NuKeeper.Inspection.Logging
{
    public class FileLogger : IInternalLogger
    {
        private readonly string _filePath;
        private readonly LogLevel _logLevel;

        public FileLogger(string filePath, LogLevel logLevel)
        {
            _filePath = filePath;
            _logLevel = logLevel;
        }

        public void Error(string message, Exception ex = null)
        {
            if (ex == null)
            {
                WriteMessageToFile(message);
            }
            else
            {
                Log(LogLevel.Quiet, $"{message} {ex.GetType().Name} : {ex.Message}");
                if (_logLevel == LogLevel.Detailed)
                {
                    WriteMessageToFile(ex.StackTrace);
                }
            }
        }

        public void Log(LogLevel level, string message)
        {
            if (_logLevel >= level)
            {
                WriteMessageToFile(message);
            }
        }

        private void WriteMessageToFile(string message)
        {
            File.AppendAllLines(_filePath, new[] {message});
        }
    }
}
