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

            var package = await lookup.FindVersionUpdate(Current("TestPackage"), VersionChange.Major);

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

            var package = await lookup.FindVersionUpdate(Current("TestPackage"), VersionChange.Major);

            AssertPackageIdentityIs(package, "TestPackage");
            Assert.That(package.Identity.Version, Is.EqualTo(new NuGetVersion(2, 3, 4)));
        }

        [Test]
        public async Task ShouldPickLatestMajorFromMultipleVersions()
        {
            var resultPackages = SeveralVersions();

            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var package = await lookup.FindVersionUpdate(Current("TestPackage"), VersionChange.Major);

            AssertPackageIdentityIs(package, "TestPackage");
            Assert.That(package.Identity.Version, Is.EqualTo(new NuGetVersion(2, 3, 4)));
        }

        [Test]
        public async Task ShouldPickLatestMinorFromMultipleVersions()
        {
            var resultPackages = SeveralVersions();

            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var package = await lookup.FindVersionUpdate(Current("TestPackage"), VersionChange.Minor);

            AssertPackageIdentityIs(package, "TestPackage");
            Assert.That(package.Identity.Version, Is.EqualTo(new NuGetVersion(1, 3, 1)));
        }

        [Test]
        public async Task ShouldPickLatestPatchFromMultipleVersions()
        {
            var resultPackages = SeveralVersions();

            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var package = await lookup.FindVersionUpdate(Current("TestPackage"), VersionChange.Minor);

            AssertPackageIdentityIs(package, "TestPackage");
            Assert.That(package.Identity.Version, Is.EqualTo(new NuGetVersion(1, 2, 5)));
        }


        private static List<PackageSearchMedatadataWithSource> SeveralVersions()
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

        private PackageIdentity Current(string packageId)
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