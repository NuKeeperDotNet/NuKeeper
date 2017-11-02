namespace NuKeeper.NuGet.Api
{
    public class PackageLookupResult
    {
        public VersionChange AllowedChange { get; set; }

        public PackageSearchMedatadata Highest { get; set; }
        public PackageSearchMedatadata Match { get; set; }
    }
}