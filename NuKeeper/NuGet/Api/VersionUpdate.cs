namespace NuKeeper.NuGet.Api
{
    public class VersionUpdate
    {
        public PackageSearchMedatadataWithSource Highest { get; set; }
        public PackageSearchMedatadataWithSource HighestMatch { get; set; }
    }
}