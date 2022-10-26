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
    public class UsePreleaseLookupTests
    {
        [TestCase(false, VersionChange.Major, 2, 3, 4, "")]
        [TestCase(false, VersionChange.Minor, 1, 3, 1, "")]
        [TestCase(false, VersionChange.Patch, 1, 2, 5, "")]
        [TestCase(false, VersionChange.None, 1, 2, 3, "")]
        [TestCase(true, VersionChange.Major, 2, 3, 4, PackageVersionTestData.PrereleaseLabel)]
        [TestCase(true, VersionChange.Minor, 1, 3, 1, PackageVersionTestData.PrereleaseLabel)]
        [TestCase(true, VersionChange.Patch, 1, 2, 5, PackageVersionTestData.PrereleaseLabel)]
        [TestCase(true, VersionChange.None, 1, 2, 3, PackageVersionTestData.PrereleaseLabel)]
        public async Task WhenFromPrereleaseIsAllowedFromPrereleaseAndCurrentVersionIsPrerelease(
            bool latestPackageIsPrerelease,
            VersionChange dataRange,
            int expectedMajor, int expectedMinor, int expectedPatch, string expectedReleaseLabel)
        {
            var expectedUpdate = new NuGetVersion(expectedMajor, expectedMinor, expectedPatch, expectedReleaseLabel);
            var resultPackages = PackageVersionTestData.VersionsFor(dataRange);
            if (latestPackageIsPrerelease)
            {
                // Only grab updated prerelease packages for this test - otherwise we'll upgrade to 2.3.4 instead of 2.3.4-prerelease
                resultPackages = resultPackages.Where(x => x.Identity.Version.IsPrerelease).ToList();
            }
            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var updates = await lookup.FindVersionUpdate(
                CurrentVersion123Prerelease("TestPackage"),
                NuGetSources.GlobalFeed,
                VersionChange.Major,
                UsePrerelease.FromPrerelease,
                true);

            AssertPackagesIdentityIs(updates, "TestPackage");
            Assert.That(updates.Selected().Identity.Version, Is.EqualTo(expectedUpdate));
            Assert.That(updates.Major.Identity.Version, Is.EqualTo(HighestVersion(resultPackages)));
        }

        [TestCase(VersionChange.Major, 2, 3, 4, "")]
        [TestCase(VersionChange.Minor, 1, 3, 1, "")]
        [TestCase(VersionChange.Patch, 1, 2, 5, "")]
        [TestCase(VersionChange.None, 1, 2, 3, "")]
        public async Task WhenFromPrereleaseIsAllowedFromPrereleaseAndCurrentVersionIsStable(VersionChange dataRange,
            int expectedMajor, int expectedMinor, int expectedPatch, string expectedReleaseLabel)
        {
            var expectedUpdate = new NuGetVersion(expectedMajor, expectedMinor, expectedPatch, expectedReleaseLabel);
            var resultPackages = PackageVersionTestData.VersionsFor(dataRange);
            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var updates = await lookup.FindVersionUpdate(
                CurrentVersion123("TestPackage"),
                NuGetSources.GlobalFeed,
                VersionChange.Major,
                UsePrerelease.FromPrerelease,
                true);

            AssertPackagesIdentityIs(updates, "TestPackage");
            Assert.That(updates.Selected().Identity.Version, Is.EqualTo(expectedUpdate));
            Assert.That(updates.Major.Identity.Version, Is.EqualTo(HighestVersion(resultPackages)));
        }

        [TestCase(false, VersionChange.Major, 2, 3, 4, "")]
        [TestCase(false, VersionChange.Minor, 1, 3, 1, "")]
        [TestCase(false, VersionChange.Patch, 1, 2, 5, "")]
        [TestCase(false, VersionChange.None, 1, 2, 3, "")]
        [TestCase(true, VersionChange.Major, 2, 3, 4, PackageVersionTestData.PrereleaseLabel)]
        [TestCase(true, VersionChange.Minor, 1, 3, 1, PackageVersionTestData.PrereleaseLabel)]
        [TestCase(true, VersionChange.Patch, 1, 2, 5, PackageVersionTestData.PrereleaseLabel)]
        [TestCase(true, VersionChange.None, 1, 2, 3, PackageVersionTestData.PrereleaseLabel)]
        public async Task WhenFromPrereleaseIsAlwaysAllowedAndCurrentVersionIsPrerelease(
            bool latestPackageIsPrerelease,
            VersionChange dataRange,
            int expectedMajor, int expectedMinor, int expectedPatch, string expectedReleaseLabel)
        {
            var expectedUpdate = new NuGetVersion(expectedMajor, expectedMinor, expectedPatch, expectedReleaseLabel);
            var resultPackages = PackageVersionTestData.VersionsFor(dataRange);
            if (latestPackageIsPrerelease)
            {
                // Only grab updated prerelease packages for this test - otherwise we'll upgrade to 2.3.4 instead of 2.3.4-prerelease
                resultPackages = resultPackages.Where(x => x.Identity.Version.IsPrerelease).ToList();
            }
            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var updates = await lookup.FindVersionUpdate(
                CurrentVersion123Prerelease("TestPackage"),
                NuGetSources.GlobalFeed,
                VersionChange.Major,
                UsePrerelease.Always,
                true);

            AssertPackagesIdentityIs(updates, "TestPackage");
            Assert.That(updates.Selected().Identity.Version, Is.EqualTo(expectedUpdate));
            Assert.That(updates.Major.Identity.Version, Is.EqualTo(HighestVersion(resultPackages)));
        }

        [TestCase(false, VersionChange.Major, 2, 3, 4, "")]
        [TestCase(false, VersionChange.Minor, 1, 3, 1, "")]
        [TestCase(false, VersionChange.Patch, 1, 2, 5, "")]
        [TestCase(false, VersionChange.None, 1, 2, 3, "")]
        [TestCase(true, VersionChange.Major, 2, 3, 4, PackageVersionTestData.PrereleaseLabel)]
        [TestCase(true, VersionChange.Minor, 1, 3, 1, PackageVersionTestData.PrereleaseLabel)]
        [TestCase(true, VersionChange.Patch, 1, 2, 5, PackageVersionTestData.PrereleaseLabel)]
        [TestCase(true, VersionChange.None, 1, 2, 3, PackageVersionTestData.PrereleaseLabel)]
        public async Task WhenFromPrereleaseIsAlwaysAllowedAndCurrentVersionIsStable(
            bool latestPackageIsPrerelease,
            VersionChange dataRange,
            int expectedMajor, int expectedMinor, int expectedPatch, string expectedReleaseLabel)
        {
            var expectedUpdate = new NuGetVersion(expectedMajor, expectedMinor, expectedPatch, expectedReleaseLabel);
            var resultPackages = PackageVersionTestData.VersionsFor(dataRange);
            if (latestPackageIsPrerelease)
            {
                // Only grab updated prerelease packages for this test - otherwise we'll upgrade to 2.3.4 instead of 2.3.4-prerelease
                resultPackages = resultPackages.Where(x => x.Identity.Version.IsPrerelease).ToList();
            }
            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var updates = await lookup.FindVersionUpdate(
                CurrentVersion123("TestPackage"),
                NuGetSources.GlobalFeed,
                VersionChange.Major,
                UsePrerelease.Always,
                true);

            AssertPackagesIdentityIs(updates, "TestPackage");
            Assert.That(updates.Selected().Identity.Version, Is.EqualTo(expectedUpdate));
            Assert.That(updates.Major.Identity.Version, Is.EqualTo(HighestVersion(resultPackages)));
        }

        [TestCase(VersionChange.Major, 2, 3, 4, "")]
        [TestCase(VersionChange.Minor, 1, 3, 1, "")]
        [TestCase(VersionChange.Patch, 1, 2, 5, "")]
        [TestCase(VersionChange.None, 1, 2, 3, "")]
        public async Task WhenFromPrereleaseIsNeverAllowedAndCurrentVersionIsPrerelease(
            VersionChange dataRange,
            int expectedMajor, int expectedMinor, int expectedPatch, string expectedReleaseLabel)
        {
            var expectedUpdate = new NuGetVersion(expectedMajor, expectedMinor, expectedPatch, expectedReleaseLabel);
            var resultPackages = PackageVersionTestData.VersionsFor(dataRange);
            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var updates = await lookup.FindVersionUpdate(
                CurrentVersion123Prerelease("TestPackage"),
                NuGetSources.GlobalFeed,
                VersionChange.Major,
                UsePrerelease.Never,
                true);

            AssertPackagesIdentityIs(updates, "TestPackage");
            Assert.That(updates.Selected().Identity.Version, Is.EqualTo(expectedUpdate));
            Assert.That(updates.Major.Identity.Version, Is.EqualTo(HighestVersion(resultPackages)));
        }

        [TestCase(VersionChange.Major, 2, 3, 4, "")]
        [TestCase(VersionChange.Minor, 1, 3, 1, "")]
        [TestCase(VersionChange.Patch, 1, 2, 5, "")]
        [TestCase(VersionChange.None, 1, 2, 3, "")]
        public async Task WhenFromPrereleaseIsNeverAllowedAndCurrentVersionIsStable(
            VersionChange dataRange,
            int expectedMajor, int expectedMinor, int expectedPatch, string expectedReleaseLabel)
        {
            var expectedUpdate = new NuGetVersion(expectedMajor, expectedMinor, expectedPatch, expectedReleaseLabel);
            var resultPackages = PackageVersionTestData.VersionsFor(dataRange);
            var allVersionsLookup = MockVersionLookup(resultPackages);

            IApiPackageLookup lookup = new ApiPackageLookup(allVersionsLookup);

            var updates = await lookup.FindVersionUpdate(
                CurrentVersion123("TestPackage"),
                NuGetSources.GlobalFeed,
                VersionChange.Major,
                UsePrerelease.Never,
                true);

            AssertPackagesIdentityIs(updates, "TestPackage");
            Assert.That(updates.Selected().Identity.Version, Is.EqualTo(expectedUpdate));
            Assert.That(updates.Major.Identity.Version, Is.EqualTo(HighestVersion(resultPackages)));
        }

        private static IPackageVersionsLookup MockVersionLookup(List<PackageSearchMetadata> actualResults)
        {
            var allVersions = Substitute.For<IPackageVersionsLookup>();
            allVersions.Lookup(Arg.Any<string>(), false, true, Arg.Any<NuGetSources>())
                .Returns(actualResults.Where(x => !x.Identity.Version.IsPrerelease).ToList());
            allVersions.Lookup(Arg.Any<string>(), true, true, Arg.Any<NuGetSources>())
                .Returns(actualResults);
            return allVersions;
        }

        private static PackageIdentity CurrentVersion123(string packageId)
        {
            return new PackageIdentity(packageId, new NuGetVersion(1, 2, 3));
        }

        private static PackageIdentity CurrentVersion123Prerelease(string packageId)
        {
            return new PackageIdentity(packageId, new NuGetVersion(1, 2, 3, PackageVersionTestData.PrereleaseLabel));
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
