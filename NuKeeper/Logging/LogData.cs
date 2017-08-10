namespace NuKeeper.Logging
{
    public class LogData
    {
        public string Terse { get; set; }
        public string Info { get; set; }
    }

    public static class LoggerExtensions
    {
        public static void Log(this INuKeeperLogger logger, LogData data)
        {
            if (!string.IsNullOrWhiteSpace(data.Terse))
            {
                logger.Terse(data.Terse);
            }

            if (!string.IsNullOrWhiteSpace(data.Info))
            {
                logger.Info(data.Info);
            }
        }
    }

}