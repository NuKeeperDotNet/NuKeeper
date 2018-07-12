namespace NuKeeper.Inspection.Logging
{
    public enum LogLevel
    {
        Silent = 0,
        Terse = 1,
        Info = 2,
        Verbose = 3,
        Quiet = Silent,
        Minimal = Terse,
        Normal = Info,
        Detailed = Verbose
    }
}
