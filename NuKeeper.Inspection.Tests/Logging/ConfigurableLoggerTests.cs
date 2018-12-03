using System;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.Logging
{
    [TestFixture]
    public class ConfigurableLoggerTests
    {
        [TestCase(LogLevel.Detailed)]
        [TestCase(LogLevel.Normal)]
        [TestCase(LogLevel.Minimal)]
        [TestCase(LogLevel.Quiet)]
        public void CanLogMessage(LogLevel loggerLevel)
        {
            var logger = new ConfigurableLogger();
            logger.Initialise(loggerLevel, LogDestination.Console, string.Empty);

            logger.Detailed("test message");
        }

        [TestCase(LogLevel.Detailed)]
        [TestCase(LogLevel.Normal)]
        [TestCase(LogLevel.Minimal)]
        [TestCase(LogLevel.Quiet)]
        public void CanLogMinimal(LogLevel loggerLevel)
        {
            var logger = new ConfigurableLogger();
            logger.Initialise(loggerLevel, LogDestination.Console, string.Empty);

            logger.Minimal("test message");
        }

        [TestCase(LogLevel.Detailed)]
        [TestCase(LogLevel.Normal)]
        [TestCase(LogLevel.Minimal)]
        [TestCase(LogLevel.Quiet)]
        public void CanLogNormal(LogLevel loggerLevel)
        {
            var logger = new ConfigurableLogger();
            logger.Initialise(loggerLevel, LogDestination.Console, string.Empty);

            logger.Normal("test message");
        }

        [TestCase(LogLevel.Detailed)]
        [TestCase(LogLevel.Normal)]
        [TestCase(LogLevel.Minimal)]
        [TestCase(LogLevel.Quiet)]
        public void CanLogError(LogLevel loggerLevel)
        {
            var logger = new ConfigurableLogger();
            logger.Initialise(loggerLevel, LogDestination.Console, string.Empty);

            logger.Error("test message", null);
        }

        [TestCase(LogLevel.Detailed)]
        [TestCase(LogLevel.Normal)]
        [TestCase(LogLevel.Minimal)]
        [TestCase(LogLevel.Quiet)]
        public void CanLogErrorWithException(LogLevel loggerLevel)
        {
            var logger = new ConfigurableLogger();
            logger.Initialise(loggerLevel, LogDestination.Console, string.Empty);

            logger.Error("test message", new ArgumentException("test"));
        }

        [TestCase(LogLevel.Detailed)]
        [TestCase(LogLevel.Normal)]
        [TestCase(LogLevel.Minimal)]
        [TestCase(LogLevel.Quiet)]
        public void CanLogErrorWithInnerException(LogLevel loggerLevel)
        {
            var logger = new ConfigurableLogger();
            logger.Initialise(loggerLevel, LogDestination.Console, string.Empty);

            logger.Error("test message", new InvalidOperationException("op test",
                new ArgumentException("arg test")));
        }

    }
}
