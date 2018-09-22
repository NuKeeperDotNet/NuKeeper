using System;

namespace NuKeeper.Inspection.Logging
{
    public class ConfigurableLogger : INuKeeperLogger, IConfigureLogger
    {
        private IInternalLogger _inner;

        public void Initialise(LogLevel logLevel, string filePath)
        {
            var destination = string.IsNullOrWhiteSpace(filePath) ?
                LogDestination.Console :
                LogDestination.File;

            _inner = CreateLogger(logLevel, destination, filePath);
        }

        public void Error(string message, Exception ex = null)
        {
            CheckLoggerCreated();
            _inner.Error(message, ex);
        }

        public void Minimal(string message)
        {
            CheckLoggerCreated();
            _inner.Log(LogLevel.Minimal, message);
        }

        public void Normal(string message)
        {
            CheckLoggerCreated();
            _inner.Log(LogLevel.Normal, message);
        }

        public void Detailed(string message)
        {
            CheckLoggerCreated();
            _inner.Log(LogLevel.Detailed, message);
        }

        private void CheckLoggerCreated()
        {
            if (_inner == null)
            {
                _inner = CreateLogger(LogLevel.Detailed, LogDestination.Console, string.Empty);
            }
        }

        private static IInternalLogger CreateLogger(
            LogLevel logLevel, LogDestination destination,
            string filePath)
        {
            switch (destination)
            {
                case LogDestination.Console:
                    return new ConsoleLogger(logLevel);

                case LogDestination.File:
                    return new FileLogger(filePath, logLevel);

                default:
                    throw new Exception($"Unknown log destination {destination}");
            }
        }
    }
}
