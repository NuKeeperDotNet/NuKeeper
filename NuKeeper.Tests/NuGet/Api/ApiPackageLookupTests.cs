using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuKeeper.NuGet.Api;
using NUnit.Framework;

namespace NuKeeper.Tests.NuGet.Api
{
    [TestFixture]
    public class ApiPackageLookupTests
    {
        [Test]
        public async Task WhenNoPackagesAreFound()
        {
            var allVersionsLookup = MockVersionLookup(new List<PackageSearchMedatadataWithSource>());
            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var package = await lookup.FindVersionUpdate(CurrentVersion123("TestPackage"), VersionChange.Major);

            Assert.That(package, Is.Null);
        }

        [Test]
        public async Task WhenThereIsOnlyOneVersion()
        {
            var resultPackages = new List<PackageSearchMedatadataWithSource>
            {
                BuildMetadata("TestPackage", 2, 3, 4)
            };

            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var package = await lookup.FindVersionUpdate(CurrentVersion123("TestPackage"), VersionChange.Major);

            AssertPackageIdentityIs(package, "TestPackage");
            Assert.That(package.Identity.Version, Is.EqualTo(new NuGetVersion(2, 3, 4)));
        }

        [TestCase(VersionChange.Major, 2, 3, 4)]
        [TestCase(VersionChange.Minor, 1, 3, 1)]
        [TestCase(VersionChange.Patch, 1, 2, 5)]
        public async Task WhenMajorVersionChangesAreAllowed(VersionChange dataRange,
            int expectedMajor, int expectedMinor, int expectedPatch)
        {
            var expectedUpdate = new NuGetVersion(expectedMajor, expectedMinor, expectedPatch);
            var resultPackages = VersionsFor(dataRange);
            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var package = await lookup.FindVersionUpdate(CurrentVersion123("TestPackage"), 
                VersionChange.Major);

            AssertPackageIdentityIs(package, "TestPackage");
            Assert.That(package.Identity.Version, Is.EqualTo(expectedUpdate));
        }

        [TestCase(VersionChange.Major, 1, 3, 1)]
        [TestCase(VersionChange.Minor, 1, 3, 1)]
        [TestCase(VersionChange.Patch, 1, 2, 5)]
        public async Task WhenMinorVersionChangesAreAllowed(VersionChange dataRange,
            int expectedMajor, int expectedMinor, int expectedPatch)
        {
            var expectedUpdate = new NuGetVersion(expectedMajor, expectedMinor, expectedPatch);
            var resultPackages = VersionsFor(dataRange);
            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var package = await lookup.FindVersionUpdate(CurrentVersion123("TestPackage"),
                VersionChange.Minor);

            AssertPackageIdentityIs(package, "TestPackage");
            Assert.That(package.Identity.Version, Is.EqualTo(expectedUpdate));
        }

        [TestCase(VersionChange.Major, 1, 2, 5)]
        [TestCase(VersionChange.Minor, 1, 2, 5)]
        [TestCase(VersionChange.Patch, 1, 2, 5)]
        public async Task WhenPatchVersionChangesAreAllowed(VersionChange dataRange,
            int expectedMajor, int expectedMinor, int expectedPatch)
        {
            var expectedUpdate = new NuGetVersion(expectedMajor, expectedMinor, expectedPatch);
            var resultPackages = VersionsFor(dataRange);
            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var package = await lookup.FindVersionUpdate(CurrentVersion123("TestPackage"),
                VersionChange.Patch);

            AssertPackageIdentityIs(package, "TestPackage");
            Assert.That(package.Identity.Version, Is.EqualTo(expectedUpdate));
        }

        private static List<PackageSearchMedatadataWithSource> VersionsFor(VersionChange change)
        {
            switch (change)
            {
                case VersionChange.Major:
                    return AllKindsOfVersions();

                case VersionChange.Minor:
                    return MinorVersions();

                case VersionChange.Patch:
                    return PatchVersions();

                default:
                    return new List<PackageSearchMedatadataWithSource>();
            }
        }


        private static List<PackageSearchMedatadataWithSource> AllKindsOfVersions()
        {
            return new List<PackageSearchMedatadataWithSource>
            {
                BuildMetadata("TestPackage", 2, 3, 4),
                BuildMetadata("TestPackage", 2, 1, 1),
                BuildMetadata("TestPackage", 2, 0, 0),

                BuildMetadata("TestPackage", 1, 3, 1),
                BuildMetadata("TestPackage", 1, 3, 0),

                BuildMetadata("TestPackage", 1, 2, 5),
                BuildMetadata("TestPackage", 1, 2, 4),
                BuildMetadata("TestPackage", 1, 2, 3),
                BuildMetadata("TestPackage", 1, 2, 2),
                BuildMetadata("TestPackage", 1, 2, 1),

                BuildMetadata("TestPackage", 1, 1, 0),
                BuildMetadata("TestPackage", 1, 0, 0)
            };
        }

        private static List<PackageSearchMedatadataWithSource> MinorVersions()
        {
            return new List<PackageSearchMedatadataWithSource>
            {
                BuildMetadata("TestPackage", 1, 3, 1),
                BuildMetadata("TestPackage", 1, 3, 0),

                BuildMetadata("TestPackage", 1, 2, 5),
                BuildMetadata("TestPackage", 1, 2, 4),
                BuildMetadata("TestPackage", 1, 2, 3),
                BuildMetadata("TestPackage", 1, 2, 2),
                BuildMetadata("TestPackage", 1, 2, 1),

                BuildMetadata("TestPackage", 1, 1, 0),
                BuildMetadata("TestPackage", 1, 0, 0)
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

                BuildMetadata("TestPackage", 1, 1, 0),
                BuildMetadata("TestPackage", 1, 0, 0)
            };
        }

        private static IPackageVersionsLookup MockVersionLookup(List<PackageSearchMedatadataWithSource> actualResults)
        {
            var allVersions = Substitute.For<IPackageVersionsLookup>();
            allVersions.Lookup(Arg.Any<string>())
                .Returns(actualResults);
            return allVersions;
        }

        private static PackageSearchMedatadataWithSource BuildMetadata(string source, int major, int minor, int patch)
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

        private PackageIdentity CurrentVersion123(string packageId)
        {
            return new PackageIdentity(packageId, new NuGetVersion(1, 2, 3));
        }

        private static void AssertPackageIdentityIs(PackageSearchMedatadataWithSource package, string id)
        {
            Assert.That(package, Is.Not.Null);
            Assert.That(package.Identity, Is.Not.Null);
            Assert.That(package.Identity.Id, Is.EqualTo(id));
        }
    }
}