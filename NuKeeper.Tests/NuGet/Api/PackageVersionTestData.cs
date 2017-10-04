using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuKeeper.NuGet.Api;

namespace NuKeeper.Tests.NuGet.Api
{
    public static class PackageVersionTestData
    {
        public static List<PackageSearchMedatadataWithSource> VersionsFor(VersionChange change)
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

                default:
                    return new List<PackageSearchMedatadataWithSource>();
            }
        }


        private static List<PackageSearchMedatadataWithSource> NewMajorVersion()
        {
            return new List<PackageSearchMedatadataWithSource>
                {
                    BuildMetadata("TestPackage", 2, 3, 4),
                    BuildMetadata("TestPackage", 2, 3, 1),
                    BuildMetadata("TestPackage", 2, 1, 1),
                    BuildMetadata("TestPackage", 2, 1, 0),
                    BuildMetadata("TestPackage", 2, 0, 1),
                    BuildMetadata("TestPackage", 2, 0, 0)
                };
        }

        private static List<PackageSearchMedatadataWithSource> MinorVersions()
        {
            return new List<PackageSearchMedatadataWithSource>
                {
                    BuildMetadata("TestPackage", 1, 3, 1),
                    BuildMetadata("TestPackage", 1, 3, 0)
                };
        }

        private static List<PackageSearchMedatadataWithSource> PatchVersions()
        {
            return new List<PackageSearchMedatadataWithSource>
            {
                BuildMetadata("TestPackage", 1, 2, 5),
                BuildMetadata("TestPackage", 1, 2, 4),
                BuildMetadata("TestPackage", 1, 2, 3),
                BuildMetadata("TestPackage", 1, 2, 2),
                BuildMetadata("TestPackage", 1, 2, 1),
                BuildMetadata("TestPackage", 1, 2, 0),

                BuildMetadata("TestPackage", 1, 1, 0),
                BuildMetadata("TestPackage", 1, 0, 0)
            };
        }

        public static PackageSearchMedatadataWithSource BuildMetadata(string source, int major, int minor, int patch)
        {
            var version = new NuGetVersion(major, minor, patch);
            var metadata = MetadataWithVersion(source, version);
            return new PackageSearchMedatadataWithSource(source, metadata);
        }

        private static IPackageSearchMetadata MetadataWithVersion(string id, NuGetVersion version)
        {
            var metadata = Substitute.For<IPackageSearchMetadata>();
            var identity = new PackageIdentity(id, version);
            metadata.Identity.Returns(identity);
            return metadata;
        }
    }
}