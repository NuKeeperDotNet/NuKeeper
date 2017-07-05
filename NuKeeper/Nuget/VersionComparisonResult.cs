namespace NuKeeper.Nuget
{
    public class VersionComparisonResult
    {
        public VersionComparisonResult(string oldVersion, string newVersion)
        {
            OldVersion = oldVersion;
            NewVersion = newVersion;
            IsUpToDate = oldVersion == newVersion;
        }

        public bool IsUpToDate { get; }
        public string OldVersion { get; }
        public string NewVersion { get; }
    }
}