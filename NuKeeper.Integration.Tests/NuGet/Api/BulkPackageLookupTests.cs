using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection.NuGetApi;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuKeeper.Integration.Tests.NuGet.Api
{
    [TestFixture]
    public class BulkPackageLookupTests : TestWithFailureLogging
    {
        [Test]
        public async Task CanFindUpdateForOneWellKnownPackage()
        {
            var packages = new List<PackageIdentity> { Current("Moq") };

            var lookup = BuildBulkPackageLookup();

            var results = await lookup.FindVersionUpdates(
                packages, NuGetSources.GlobalFeed, VersionChange.Major,
                UsePrerelease.FromPrerelease);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results, Does.ContainKey("Moq"));
        }

        [Test]
        public async Task CanFindUpdateForTwoWellKnownPackages()
        {
            var packages = new List<PackageIdentity>
            {
                Current("Moq"),
                Current("Newtonsoft.Json")
            };

            var lookup = BuildBulkPackageLookup();

            var results = await lookup.FindVersionUpdates(
                packages, NuGetSources.GlobalFeed, VersionChange.Major,
                UsePrerelease.FromPrerelease);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results, Does.ContainKey("Moq"));
            Assert.That(results, Does.ContainKey("Newtonsoft.Json"));
        }

        [Test]
        public async Task FindsSingleUpdateForPackageDifferingOnlyByCase()
        {
            var packages = new List<PackageIdentity>
            {
                Current("nunit"),
                Current("NUnit")
            };

            var lookup = BuildBulkPackageLookup();

            var results = await lookup.FindVersionUpdates(
                packages, NuGetSources.GlobalFeed, VersionChange.Major,
                UsePrerelease.FromPrerelease);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.ContainsKey("NUnit"), "results.ContainsKey('NUnit')");
            Assert.That(results.ContainsKey("nunit"), "results.ContainsKey('nunit')");
        }

        [Test]
        public async Task InvalidPackageIsIgnored()
        {
            var packages = new List<PackageIdentity>
            {
                Current(Guid.NewGuid().ToString())
            };

            var lookup = BuildBulkPackageLookup();

            var results = await lookup.FindVersionUpdates(
                packages, NuGetSources.GlobalFeed, VersionChange.Major,
                UsePrerelease.FromPrerelease);

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task TestEmptyList()
        {
            var lookup = BuildBulkPackageLookup();

            var results = await lookup.FindVersionUpdates(
                Enumerable.Empty<PackageIdentity>(), NuGetSources.GlobalFeed, VersionChange.Major,
                UsePrerelease.FromPrerelease);

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task ValidPackagesWorkDespiteInvalidPackages()
        {
            var packages = new List<PackageIdentity>
            {
                Current("Moq"),
                Current(Guid.NewGuid().ToString()),
                Current("Newtonsoft.Json"),
                Current(Guid.NewGuid().ToString())
            };

            var lookup = BuildBulkPackageLookup();

            var results = await lookup.FindVersionUpdates(
                packages, NuGetSources.GlobalFeed, VersionChange.Major,
                UsePrerelease.FromPrerelease);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results, Does.ContainKey("Moq"));
            Assert.That(results, Does.ContainKey("Newtonsoft.Json"));
        }

        private BulkPackageLookup BuildBulkPackageLookup()
        {
            var lookup = new ApiPackageLookup(new PackageVersionsLookup(NugetLogger, NukeeperLogger));
            return new BulkPackageLookup(lookup, new PackageLookupResultReporter(NukeeperLogger));
        }

        private static PackageIdentity Current(string packageId)
        {
            return new PackageIdentity(packageId, new NuGetVersion(1, 2, 3));
        }
    }
}
