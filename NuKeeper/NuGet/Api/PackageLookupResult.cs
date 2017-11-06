namespace NuKeeper.NuGet.Api
{
    public class PackageLookupResult
    {
        public PackageLookupResult(
            VersionChange allowedChange, 
            PackageSearchMedatadata highest, PackageSearchMedatadata match)
        {
            AllowedChange = allowedChange;
            Highest = highest;
            Match = match;
        }

        public VersionChange AllowedChange { get; }
        public PackageSearchMedatadata Highest { get; }
        public PackageSearchMedatadata Match { get; }
    }
}