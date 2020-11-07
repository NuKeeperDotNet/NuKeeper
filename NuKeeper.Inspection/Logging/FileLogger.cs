using NuKeeper.Abstractions.Logging;
using System;
using System.IO;

namespace NuKeeper.Inspection.Logging
{
    public class FileLogger : IInternalLogger
    {
        private readonly object _fileLocker = new object();
        private readonly string _filePath;
        private readonly LogLevel _logLevel;

        public FileLogger(string filePath, LogLevel logLevel)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File logger has no file path", nameof(filePath));
            }

            _filePath = filePath;
            _logLevel = logLevel;
        }

        public void LogError(string message, Exception ex)
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
            lock (_fileLocker)
            {
                using var w = File.AppendText(_filePath);
                w.WriteLine(message);
            }
        }
    }
}
