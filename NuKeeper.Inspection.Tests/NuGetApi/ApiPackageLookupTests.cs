using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.NuGetApi;
using NuKeeper.Inspection.NuGetApi;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.NuGetApi
{
    [TestFixture]
    public class ApiPackageLookupTests
    {
        [Test]
        public async Task WhenNoPackagesAreFound()
        {
            var allVersionsLookup = MockVersionLookup(new List<PackageSearchMetadata>());
            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var updates = await lookup.FindVersionUpdate(
                CurrentVersion123("TestPackage"),
                NuGetSources.GlobalFeed,
                VersionChange.Major,
                UsePrerelease.FromPrerelease);

            Assert.That(updates, Is.Not.Null);
            Assert.That(updates.Major, Is.Null);
            Assert.That(updates.Selected(), Is.Null);
        }

        [Test]
        public async Task WhenThereIsOnlyOneVersion()
        {
            var resultPackages = new List<PackageSearchMetadata>
            {
                PackageVersionTestData.PackageVersion(2, 3, 4)
            };

            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var updates = await lookup.FindVersionUpdate(
                CurrentVersion123("TestPackage"),
                NuGetSources.GlobalFeed,
                VersionChange.Major,
                UsePrerelease.FromPrerelease);

            Assert.That(updates, Is.Not.Null);

            AssertPackagesIdentityIs(updates, "TestPackage");
            Assert.That(updates.Major.Identity.Version, Is.EqualTo(new NuGetVersion(2, 3, 4)));
            Assert.That(updates.Selected().Identity.Version, Is.EqualTo(new NuGetVersion(2, 3, 4)));
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

            var updates = await lookup.FindVersionUpdate(
                CurrentVersion123("TestPackage"),
                NuGetSources.GlobalFeed,
                VersionChange.Major,
                UsePrerelease.FromPrerelease);

            AssertPackagesIdentityIs(updates, "TestPackage");
            Assert.That(updates.Selected().Identity.Version, Is.EqualTo(expectedUpdate));
            Assert.That(updates.Major.Identity.Version, Is.EqualTo(HighestVersion(resultPackages)));
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

            var updates = await lookup.FindVersionUpdate(
                CurrentVersion123("TestPackage"),
                NuGetSources.GlobalFeed,
                VersionChange.Minor,
                UsePrerelease.FromPrerelease);

            AssertPackagesIdentityIs(updates, "TestPackage");
            Assert.That(updates.Selected().Identity.Version, Is.EqualTo(expectedUpdate));
            Assert.That(updates.Major.Identity.Version, Is.EqualTo(HighestVersion(resultPackages)));
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

            var updates = await lookup.FindVersionUpdate(
                CurrentVersion123("TestPackage"),
                NuGetSources.GlobalFeed,
                VersionChange.Patch,
                UsePrerelease.FromPrerelease);

            AssertPackagesIdentityIs(updates, "TestPackage");
            Assert.That(updates.Selected().Identity.Version, Is.EqualTo(expectedUpdate));
            Assert.That(updates.Major.Identity.Version, Is.EqualTo(HighestVersion(resultPackages)));
        }

        private static IPackageVersionsLookup MockVersionLookup(List<PackageSearchMetadata> actualResults)
        {
            var allVersions = Substitute.For<IPackageVersionsLookup>();
            allVersions.Lookup(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<NuGetSources>())
                .Returns(actualResults);
            return allVersions;
        }

        private static PackageIdentity CurrentVersion123(string packageId)
        {
            return new PackageIdentity(packageId, new NuGetVersion(1, 2, 3));
        }

        private static void AssertPackagesIdentityIs(PackageLookupResult packages, string id)
        {
            Assert.That(packages, Is.Not.Null);
            AssertPackageIdentityIs(packages.Major, id);
            AssertPackageIdentityIs(packages.Selected(), id);

            Assert.That(packages.Major.Identity.Version, Is.GreaterThanOrEqualTo(packages.Selected().Identity));
        }

        private static void AssertPackageIdentityIs(PackageSearchMetadata package, string id)
        {
            Assert.That(package, Is.Not.Null);
            Assert.That(package.Identity, Is.Not.Null);
            Assert.That(package.Identity.Id, Is.EqualTo(id));
        }

        private static NuGetVersion HighestVersion(IEnumerable<PackageSearchMetadata> packages)
        {
            return packages
                .Select(p => p.Identity.Version)
                .OrderByDescending(v => v)
                .First();
        }
    }
}
