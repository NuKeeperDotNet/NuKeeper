using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Inspection.NuGetApi;
using NUnit.Framework;

namespace NuKeeper.Tests.NuGet.Api
{
    [TestFixture]
    public class BulkPackageLookupTests
    {
        [Test]
        public async Task CanLookupEmptyList()
        {
            var apiLookup = Substitute.For<IApiPackageLookup>();
            var bulkLookup = BuildBulkPackageLookup(apiLookup);

            var results = await bulkLookup.FindVersionUpdates(Enumerable.Empty<PackageIdentity>(), VersionChange.Major);

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);

            await apiLookup.DidNotReceive().FindVersionUpdate(Arg.Any<PackageIdentity>(), Arg.Any<VersionChange>());
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

            var results = await bulkLookup.FindVersionUpdates(queries, VersionChange.Major);

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

            await bulkLookup.FindVersionUpdates(queries, VersionChange.Major);

            await apiLookup.Received(1).FindVersionUpdate(Arg.Any<PackageIdentity>(), Arg.Any<VersionChange>());
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

            var results = await bulkLookup.FindVersionUpdates(queries, VersionChange.Major);

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

            await bulkLookup.FindVersionUpdates(queries, VersionChange.Major);

            await apiLookup.Received(2).FindVersionUpdate(Arg.Any<PackageIdentity>(), Arg.Any<VersionChange>());
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

            var results = await bulkLookup.FindVersionUpdates(queries, VersionChange.Major);

            await apiLookup.Received(1).FindVersionUpdate(Arg.Any<PackageIdentity>(), Arg.Any<VersionChange>());
            await apiLookup.Received(1).FindVersionUpdate(Arg.Is<PackageIdentity>(
                pi => pi.Id == "foo" && pi.Version == new NuGetVersion(1, 3, 4)), Arg.Any<VersionChange>());

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.ContainsKey("foo"), Is.True);
            Assert.That(results.ContainsKey("bar"), Is.False);
        }

        private static void ApiHasNewVersionForPackage(IApiPackageLookup lookup, string packageName)
        {
            var responseMetaData = new PackageSearchMedatadata(
                new PackageIdentity(packageName, new NuGetVersion(2, 3, 4)), "test", DateTimeOffset.Now);

            lookup.FindVersionUpdate(Arg.Is<PackageIdentity>(pm => pm.Id == packageName), Arg.Any<VersionChange>())
                .Returns(new PackageLookupResult(VersionChange.Major, responseMetaData, responseMetaData, responseMetaData));
        }

        private static BulkPackageLookup BuildBulkPackageLookup(IApiPackageLookup apiLookup)
        {
            var logger = new NullNuKeeperLogger();
            return new BulkPackageLookup(apiLookup, new PackageLookupResultReporter(logger));
        }
    }
}
