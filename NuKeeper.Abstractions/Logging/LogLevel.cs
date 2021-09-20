namespace NuKeeper.Abstractions.Logging
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1027:Mark enums with FlagsAttribute", Justification = "False positive")]
    public enum LogLevel
    {
        Quiet = 0,
        Q = Quiet,

        Minimal = 1,
        M = Minimal,

        Normal = 2,
        N = Normal,

        Detailed = 3,
        D = Detailed
    }
}
