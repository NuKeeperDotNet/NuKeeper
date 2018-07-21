namespace NuKeeper.Inspection.Logging
{
    public enum LogLevel
    {
        Silent = 0,
        Terse = 1,
        Info = 2,
        Verbose = 3,
        Quiet = Silent,
        Q = Quiet,
        Minimal = Terse,
        M = Minimal,
        Normal = Info,
        N = Normal,
        Detailed = Verbose,
        D = Detailed
    }
}
