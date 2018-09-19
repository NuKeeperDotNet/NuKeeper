namespace NuKeeper.Inspection.Logging
{
    public interface IConfigureLogLevel
    {
        void SetLogLevel(LogLevel logLevel);
        void SetToFile(string filePath);
    }
}
