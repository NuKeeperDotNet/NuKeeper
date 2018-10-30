using NuKeeper.Abstractions.Logging;

namespace NuKeeper.Inspection.Logging
{
    public interface IConfigureLogger
    {
        void Initialise(LogLevel logLevel, LogDestination dest, string filePath);
    }
}
