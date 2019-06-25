using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.NuGetApi;

namespace NuKeeper.Inspection.Tests.NuGetApi
{
    public static class PackageVersionTestData
    {
        public const string PrereleaseLabel = "prerelease";

        public static List<PackageSearchMetadata> VersionsFor(VersionChange change)
        {
            switch (change)
            {
                case VersionChange.Major:
                    return NewMajorVersion()
                        .Concat(ConvertToPrerelease(NewMajorVersion()))
                        .Concat(MinorVersions())
                        .Concat(ConvertToPrerelease(MinorVersions()))
                        .Concat(PatchVersions())
                        .Concat(ConvertToPrerelease(PatchVersions()))
                        .ToList();

                case VersionChange.Minor:
                    return MinorVersions()
                        .Concat(ConvertToPrerelease(MinorVersions()))
                        .Concat(PatchVersions())
                        .Concat(ConvertToPrerelease(PatchVersions()))
                        .ToList();

                case VersionChange.Patch:
                    return PatchVersions()
                        .Concat(ConvertToPrerelease(PatchVersions()))
                        .ToList();

                case VersionChange.None:
                    return CurrentVersionOnly()
                        .Concat(ConvertToPrerelease(CurrentVersionOnly()))
                        .ToList();

                default:
                    throw new ArgumentOutOfRangeException($"Invalid version change {change}");
            }
        }

        private static List<PackageSearchMetadata> ConvertToPrerelease(List<PackageSearchMetadata> packages)
        {
            return packages.Select(p =>
                PackageVersion(p.Identity.Version.Major, p.Identity.Version.Minor, p.Identity.Version.Patch, PrereleaseLabel))
                .ToList();

        }

        private static List<PackageSearchMetadata> NewMajorVersion()
        {
            return new List<PackageSearchMetadata>
            {
                PackageVersion(2, 3, 4),
                PackageVersion(2, 3, 1),
                PackageVersion(2, 1, 1),
                PackageVersion(2, 1, 0),
                PackageVersion(2, 0, 1),
                PackageVersion(2, 0, 0)
            };
        }

        private static List<PackageSearchMetadata> MinorVersions()
        {
            return new List<PackageSearchMetadata>
            {
                PackageVersion(1, 3, 1),
                PackageVersion(1, 3, 0)
            };
        }

        private static List<PackageSearchMetadata> PatchVersions()
        {
            return new List<PackageSearchMetadata>
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

        private static List<PackageSearchMetadata> CurrentVersionOnly()
        {
            return new List<PackageSearchMetadata>
            {
                PackageVersion(1, 2, 3)
            };
        }

        public static PackageSearchMetadata PackageVersion(int major, int minor, int patch, string releaseLabel = "")
        {
            var version = new NuGetVersion(major, minor, patch, releaseLabel);
            var metadata = new PackageIdentity("TestPackage", version);
            return new PackageSearchMetadata(metadata, new PackageSource("http://none"), DateTimeOffset.Now, null);
        }
    }
}
