using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Packaging.Core;
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

            var updates = await lookup.FindVersionUpdate(CurrentVersion123("TestPackage"), VersionChange.Major);

            Assert.That(updates, Is.Not.Null);
            Assert.That(updates.Highest, Is.Null);
            Assert.That(updates.Match, Is.Null);
        }

        [Test]
        public async Task WhenThereIsOnlyOneVersion()
        {
            var resultPackages = new List<PackageSearchMedatadataWithSource>
            {
                PackageVersionTestData.BuildMetadata("TestPackage", 2, 3, 4)
            };

            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var updates = await lookup.FindVersionUpdate(CurrentVersion123("TestPackage"), VersionChange.Major);

            Assert.That(updates, Is.Not.Null);

            AssertPackagesIdentityIs(updates, "TestPackage");
            Assert.That(updates.Highest.Identity.Version, Is.EqualTo(new NuGetVersion(2, 3, 4)));
            Assert.That(updates.Match.Identity.Version, Is.EqualTo(new NuGetVersion(2, 3, 4)));
        }

        [TestCase(VersionChange.Major, 2, 3, 4)]
        [TestCase(VersionChange.Minor, 1, 3, 1)]
        [TestCase(VersionChange.Patch, 1, 2, 5)]
        [TestCase(VersionChange.None, 1, 2, 3)]
        public async Task WhenMajorVersionChangesAreAllowed(VersionChange dataRange,
            int expectedMajor, int expectedMinor, int expectedPatch)
        {
            var expectedUpdate = new NuGetVersion(expectedMajor, expectedMinor, expectedPatch);
            var resultPackages = PackageVersionTestData.VersionsFor(dataRange);
            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var updates = await lookup.FindVersionUpdate(CurrentVersion123("TestPackage"), 
                VersionChange.Major);

            AssertPackagesIdentityIs(updates, "TestPackage");
            Assert.That(updates.Match.Identity.Version, Is.EqualTo(expectedUpdate));
            Assert.That(updates.Highest.Identity.Version, Is.EqualTo(HighestVersion(resultPackages)));
        }

        [TestCase(VersionChange.Major, 1, 3, 1)]
        [TestCase(VersionChange.Minor, 1, 3, 1)]
        [TestCase(VersionChange.Patch, 1, 2, 5)]
        [TestCase(VersionChange.None, 1, 2, 3)]
        public async Task WhenMinorVersionChangesAreAllowed(VersionChange dataRange,
            int expectedMajor, int expectedMinor, int expectedPatch)
        {
            var expectedUpdate = new NuGetVersion(expectedMajor, expectedMinor, expectedPatch);
            var resultPackages = PackageVersionTestData.VersionsFor(dataRange);
            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var updates = await lookup.FindVersionUpdate(CurrentVersion123("TestPackage"),
                VersionChange.Minor);

            AssertPackagesIdentityIs(updates, "TestPackage");
            Assert.That(updates.Match.Identity.Version, Is.EqualTo(expectedUpdate));
            Assert.That(updates.Highest.Identity.Version, Is.EqualTo(HighestVersion(resultPackages)));
        }

        [TestCase(VersionChange.Major, 1, 2, 5)]
        [TestCase(VersionChange.Minor, 1, 2, 5)]
        [TestCase(VersionChange.Patch, 1, 2, 5)]
        [TestCase(VersionChange.None, 1, 2, 3)]
        public async Task WhenPatchVersionChangesAreAllowed(VersionChange dataRange,
            int expectedMajor, int expectedMinor, int expectedPatch)
        {
            var expectedUpdate = new NuGetVersion(expectedMajor, expectedMinor, expectedPatch);
            var resultPackages = PackageVersionTestData.VersionsFor(dataRange);
            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var updates = await lookup.FindVersionUpdate(CurrentVersion123("TestPackage"),
                VersionChange.Patch);

            AssertPackagesIdentityIs(updates, "TestPackage");
            Assert.That(updates.Match.Identity.Version, Is.EqualTo(expectedUpdate));
            Assert.That(updates.Highest.Identity.Version, Is.EqualTo(HighestVersion(resultPackages)));
        }

        private static IPackageVersionsLookup MockVersionLookup(List<PackageSearchMedatadataWithSource> actualResults)
        {
            var allVersions = Substitute.For<IPackageVersionsLookup>();
            allVersions.Lookup(Arg.Any<string>())
                .Returns(actualResults);
            return allVersions;
        }

        private PackageIdentity CurrentVersion123(string packageId)
        {
            return new PackageIdentity(packageId, new NuGetVersion(1, 2, 3));
        }

        private static void AssertPackagesIdentityIs(PackageLookupResult packages, string id)
        {
            Assert.That(packages, Is.Not.Null);
            AssertPackageIdentityIs(packages.Highest, id);
            AssertPackageIdentityIs(packages.Match, id);

            Assert.That(packages.Highest.Identity.Version, Is.GreaterThanOrEqualTo(packages.Match.Identity));
        }

        private static void AssertPackageIdentityIs(PackageSearchMedatadataWithSource package, string id)
        {
            Assert.That(package, Is.Not.Null);
            Assert.That(package.Identity, Is.Not.Null);
            Assert.That(package.Identity.Id, Is.EqualTo(id));
        }

        private static NuGetVersion HighestVersion(IEnumerable<PackageSearchMedatadataWithSource> packages)
        {
            return packages
                .Select(p => p.Identity.Version)
                .OrderByDescending(v => v)
                .First();
        }
    }
}