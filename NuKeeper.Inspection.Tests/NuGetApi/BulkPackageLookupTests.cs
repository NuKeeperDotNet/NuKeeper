using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.NuGetApi;
using NuKeeper.Inspection.NuGetApi;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.NuGetApi
{
    [TestFixture]
    public class BulkPackageLookupTests
    {
        [Test]
        public async Task CanLookupEmptyList()
        {
            var apiLookup = Substitute.For<IApiPackageLookup>();
            var bulkLookup = BuildBulkPackageLookup(apiLookup);

            var results = await bulkLookup.FindVersionUpdates(
                Enumerable.Empty<PackageIdentity>(),
                NuGetSources.GlobalFeed,
                VersionChange.Major,
                UsePrerelease.FromPrerelease);

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);

            await apiLookup.DidNotReceive().FindVersionUpdate(
                Arg.Any<PackageIdentity>(), Arg.Any<NuGetSources>(), Arg.Any<VersionChange>(), Arg.Any<UsePrerelease>());
        }

        [Test]
        public async Task CanLookupOnePackage()
        {
            var apiLookup = Substitute.For<IApiPackageLookup>();

            ApiHasNewVersionForPackage(apiLookup, "foo");

            var bulkLookup = BuildBulkPackageLookup(apiLookup);

            var queries = new List<PackageIdentity>
            {
                new PackageIdentity("foo", new NuGetVersion(1, 2, 3))
            };

            var results = await bulkLookup.FindVersionUpdates(queries,
                NuGetSources.GlobalFeed,
                VersionChange.Major,
                UsePrerelease.FromPrerelease);

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Not.Empty);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.ContainsKey("foo"), Is.True);
        }

        [Test]
        public async Task LookupOnePackageCallsApiOnce()
        {
            var apiLookup = Substitute.For<IApiPackageLookup>();

            ApiHasNewVersionForPackage(apiLookup, "foo");

            var bulkLookup = BuildBulkPackageLookup(apiLookup);

            var queries = new List<PackageIdentity>
            {
                new PackageIdentity("foo", new NuGetVersion(1, 2, 3))
            };

            await bulkLookup.FindVersionUpdates(queries,
                NuGetSources.GlobalFeed,
                VersionChange.Major,
                UsePrerelease.FromPrerelease);

            await apiLookup.Received(1).FindVersionUpdate(
                Arg.Any<PackageIdentity>(), Arg.Any<NuGetSources>(), Arg.Any<VersionChange>(), Arg.Any<UsePrerelease>());
        }

        [Test]
        public async Task CanLookupTwoPackages()
        {
            var apiLookup = Substitute.For<IApiPackageLookup>();

            ApiHasNewVersionForPackage(apiLookup, "foo");
            ApiHasNewVersionForPackage(apiLookup, "bar");

            var bulkLookup = BuildBulkPackageLookup(apiLookup);

            var queries = new List<PackageIdentity>
            {
                new PackageIdentity("foo", new NuGetVersion(1, 2, 3)),
                new PackageIdentity("bar", new NuGetVersion(1, 2, 3))
            };

            var results = await bulkLookup.FindVersionUpdates(queries,
                NuGetSources.GlobalFeed,
                VersionChange.Major,
                UsePrerelease.FromPrerelease);

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.ContainsKey("foo"), Is.True);
            Assert.That(results.ContainsKey("bar"), Is.True);
            Assert.That(results.ContainsKey("fish"), Is.False);
        }

        [Test]
        public async Task LookupTwoPackagesCallsApiTwice()
        {
            var apiLookup = Substitute.For<IApiPackageLookup>();

            ApiHasNewVersionForPackage(apiLookup, "foo");
            ApiHasNewVersionForPackage(apiLookup, "bar");

            var bulkLookup = BuildBulkPackageLookup(apiLookup);

            var queries = new List<PackageIdentity>
            {
                new PackageIdentity("foo", new NuGetVersion(1, 2, 3)),
                new PackageIdentity("bar", new NuGetVersion(1, 2, 3))
            };

            await bulkLookup.FindVersionUpdates(queries,
                NuGetSources.GlobalFeed,
                VersionChange.Major,
                UsePrerelease.FromPrerelease);

            await apiLookup.Received(2).FindVersionUpdate(
                Arg.Any<PackageIdentity>(), Arg.Any<NuGetSources>(), Arg.Any<VersionChange>(), Arg.Any<UsePrerelease>());
        }

        [Test]
        public async Task WhenThereAreMultipleVersionOfTheSamePackage()
        {
            var apiLookup = Substitute.For<IApiPackageLookup>();

            ApiHasNewVersionForPackage(apiLookup, "foo");

            var bulkLookup = BuildBulkPackageLookup(apiLookup);

            var queries = new List<PackageIdentity>
            {
                new PackageIdentity("foo", new NuGetVersion(1, 2, 3)),
                new PackageIdentity("foo", new NuGetVersion(1, 3, 4))
            };

            var results = await bulkLookup.FindVersionUpdates(queries,
                NuGetSources.GlobalFeed,
                VersionChange.Major,
                UsePrerelease.FromPrerelease);

            await apiLookup.Received(1).FindVersionUpdate(
                Arg.Any<PackageIdentity>(), Arg.Any<NuGetSources>(), Arg.Any<VersionChange>(), Arg.Any<UsePrerelease>());
            await apiLookup.Received(1).FindVersionUpdate(Arg.Is<PackageIdentity>(
                pi => pi.Id == "foo" && pi.Version == new NuGetVersion(1, 3, 4)),
                Arg.Any<NuGetSources>(), Arg.Any<VersionChange>(), Arg.Any<UsePrerelease>());

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.ContainsKey("foo"), Is.True);
            Assert.That(results.ContainsKey("bar"), Is.False);
        }

        private static void ApiHasNewVersionForPackage(IApiPackageLookup lookup, string packageName)
        {
            var responseMetaData = new PackageSearchMedatadata(
                new PackageIdentity(packageName, new NuGetVersion(2, 3, 4)), new PackageSource("http://none"),
                DateTimeOffset.Now, null);

            lookup.FindVersionUpdate(Arg.Is<PackageIdentity>(pm => pm.Id == packageName),
                    Arg.Any<NuGetSources>(), Arg.Any<VersionChange>(), Arg.Any<UsePrerelease>())
                .Returns(new PackageLookupResult(VersionChange.Major, responseMetaData, responseMetaData, responseMetaData));
        }

        private static BulkPackageLookup BuildBulkPackageLookup(IApiPackageLookup apiLookup)
        {
            return new BulkPackageLookup(apiLookup, new PackageLookupResultReporter(Substitute.For<INuKeeperLogger>()));
        }
    }
}
