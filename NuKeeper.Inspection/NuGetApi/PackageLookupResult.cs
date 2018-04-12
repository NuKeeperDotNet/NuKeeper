using System;

namespace NuKeeper.Inspection.NuGetApi
{
    public class PackageLookupResult
    {
        public PackageLookupResult(
            VersionChange allowedChange,
            PackageSearchMedatadata major,
            PackageSearchMedatadata minor,
            PackageSearchMedatadata patch)
        {
            AllowedChange = allowedChange;
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public VersionChange AllowedChange { get; }

        public PackageSearchMedatadata Major { get; }
        public PackageSearchMedatadata Minor { get; }
        public PackageSearchMedatadata Patch { get; }

        public PackageSearchMedatadata Selected()
        {
            switch (AllowedChange)
            {
                case VersionChange.Major:
                    return Major;

                case VersionChange.Minor:
                    return Minor;

                case VersionChange.Patch:
                    return Patch;

                case VersionChange.None:
                    return null;

                default:
                    throw new Exception($"Unknown version change {AllowedChange}");
            }
        }
    }
}
