using System;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.Inspection.Logging
{
    public class ConfigurableLogger : INuKeeperLogger, IConfigureLogger
    {
        private IInternalLogger _inner;

        public void Initialise(LogLevel logLevel, LogDestination destination, string filePath)
        {
            _inner = CreateLogger(logLevel, destination, filePath);
        }

        public void Error(string message, Exception ex)
        {
            CheckLoggerCreated();
            _inner.LogError(message, ex);
            if (ex?.InnerException != null)
            {
                Error("Inner Exception", ex.InnerException);
            }
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

                case LogDestination.Off:
                    return new NullLogger();

                default:
                    throw new Exception($"Unknown log destination {destination}");
            }
        }
    }
}
