using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Inspection.NuGetApi;

namespace NuKeeper.Inspection.Tests.NuGetApi
{
    public static class PackageVersionTestData
    {
        public static List<PackageSearchMedatadata> VersionsFor(VersionChange change)
        {
            switch (change)
            {
                case VersionChange.Major:
                    return NewMajorVersion()
                        .Concat(MinorVersions())
                        .Concat(PatchVersions())
                        .ToList();

                case VersionChange.Minor:
                    return MinorVersions()
                        .Concat(PatchVersions())
                        .ToList();

                case VersionChange.Patch:
                    return PatchVersions();

                case VersionChange.None:
                    return CurrentVersionOnly();

                default:
                    throw new Exception($"Invalid version change {change}");
            }
        }

        private static List<PackageSearchMedatadata> NewMajorVersion()
        {
            return new List<PackageSearchMedatadata>
            {
                PackageVersion(2, 3, 4),
                PackageVersion(2, 3, 1),
                PackageVersion(2, 1, 1),
                PackageVersion(2, 1, 0),
                PackageVersion(2, 0, 1),
                PackageVersion(2, 0, 0)
            };
        }

        private static List<PackageSearchMedatadata> MinorVersions()
        {
            return new List<PackageSearchMedatadata>
            {
                PackageVersion(1, 3, 1),
                PackageVersion(1, 3, 0)
            };
        }

        private static List<PackageSearchMedatadata> PatchVersions()
        {
            return new List<PackageSearchMedatadata>
            {
                PackageVersion(1, 2, 5),
                PackageVersion(1, 2, 4),
                PackageVersion(1, 2, 3),
                PackageVersion(1, 2, 2),
                PackageVersion(1, 2, 1),
                PackageVersion(1, 2, 0),

                PackageVersion(1, 1, 0),
                PackageVersion(1, 0, 0)
            };
        }

        private static List<PackageSearchMedatadata> CurrentVersionOnly()
        {
            return new List<PackageSearchMedatadata>
            {
                PackageVersion(1, 2, 3)
            };
        }

        public static PackageSearchMedatadata PackageVersion(int major, int minor, int patch)
        {
            var version = new NuGetVersion(major, minor, patch);
            var metadata = new PackageIdentity("TestPackage", version);
            return new PackageSearchMedatadata(metadata, new PackageSource("http://none"), DateTimeOffset.Now, null);
        }
    }
}
