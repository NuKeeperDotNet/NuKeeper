using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Abstractions
{
    public class PackageLookupResult
    {
        public PackageLookupResult(
            VersionChange allowedChange,
            PackageSearchMedatadata major,
            PackageSearchMedatadata selected)
        {
            AllowedChange = allowedChange;
            Major = major;
            Selected = selected;
        }

        public VersionChange AllowedChange { get; }

        public PackageSearchMedatadata Major { get; }

        public PackageSearchMedatadata Selected { get; }
    }
}
