namespace NuKeeper.NuGet.Api
{
    public class PackageLookupResult
    {
        public VersionChange AllowedChange { get; set; }

        public PackageSearchMedatadataWithSource Highest { get; set; }
        public PackageSearchMedatadataWithSource Match { get; set; }
    }
}