using System;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Abstractions.NuGetApi
{
    public class PackageLookupResult
    {
        public PackageLookupResult(
            VersionChange allowedChange,
            PackageSearchMetadata major,
            PackageSearchMetadata minor,
            PackageSearchMetadata patch)
        {
            AllowedChange = allowedChange;
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public VersionChange AllowedChange { get; }

        public PackageSearchMetadata Major { get; }
        public PackageSearchMetadata Minor { get; }
        public PackageSearchMetadata Patch { get; }

        public PackageSearchMetadata Selected()
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
                    throw new NuKeeperException($"Unknown version change {AllowedChange}");
            }
        }
    }
}
