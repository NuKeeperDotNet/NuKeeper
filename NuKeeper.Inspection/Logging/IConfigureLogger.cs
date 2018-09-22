namespace NuKeeper.Inspection.Logging
{
    public enum LogDestination
    {
        Console,
        File
    }

    public interface IConfigureLogger
    {
        void Initialise(LogLevel logLevel, string filePath);
    }
}
