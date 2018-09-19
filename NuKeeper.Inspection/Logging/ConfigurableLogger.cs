using System;

namespace NuKeeper.Inspection.Logging
{
    public class ConfigurableLogger : INuKeeperLogger, IConfigureLogLevel
    {
        private const LogLevel DefaultLogLevel = LogLevel.Normal;

        private LogLevel _currentLogLevel = DefaultLogLevel;
        private IInternalLogger _inner = new ConsoleLogger(DefaultLogLevel);

        public void SetLogLevel(LogLevel logLevel)
        {
            if (logLevel != _currentLogLevel)
            {
                _currentLogLevel = logLevel;
                _inner = new ConsoleLogger(_currentLogLevel);
            }
        }

        public void SetToFile(string filePath)
        {
            _inner = new FileLogger(filePath, _currentLogLevel);
        }

        public void Error(string message, Exception ex = null)
        {
            _inner.Error(message, ex);
        }

        public void Minimal(string message)
        {
            _inner.Log(LogLevel.Minimal, message);
        }

        public void Normal(string message)
        {
            _inner.Log(LogLevel.Normal, message);
        }

        public void Detailed(string message)
        {
            _inner.Log(LogLevel.Detailed, message);
        }
    }
}
