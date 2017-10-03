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

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Identity, Is.Not.Null);
            Assert.That(package.Identity.Id, Is.EqualTo("TestPackage"));
            Assert.That(package.Identity.Version, Is.EqualTo(new NuGetVersion(2, 3, 4)));
        }

        [Test]
        public async Task ShouldPickLatestFromMultipleVersions()
        {
            var resultPackages = new List<PackageSearchMedatadataWithSource>
            {
                BuildMetadata("TestPackage", 2,3,4),
                BuildMetadata("TestPackage", 1,23,55),
                BuildMetadata("TestPackage", 8, 0,8),
                BuildMetadata("TestPackage", 0,1,904)
            };

            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var package = await lookup.FindVersionUpdate(Current("TestPackage"), VersionChange.Major);

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Identity, Is.Not.Null);
            Assert.That(package.Identity.Id, Is.EqualTo("TestPackage"));
            Assert.That(package.Identity.Version, Is.EqualTo(new NuGetVersion(8, 0, 8)));
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
    }
}