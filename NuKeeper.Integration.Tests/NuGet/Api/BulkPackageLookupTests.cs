using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Sources;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.NuGet.Api
{
    [TestFixture]
    public class BulkPackageLookupTests
    {
        [Test]
        public async Task CanFindUpdateForOneWellKnownPackage()
        {
            var packages = new List<PackageIdentity> { Current("Moq") };

            var lookup = BuildBulkPackageLookup();

            var results = await lookup.FindVersionUpdates(
                packages, NuGetSources.GlobalFeed, VersionChange.Major);

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
                packages, NuGetSources.GlobalFeed, VersionChange.Major);

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
                packages, NuGetSources.GlobalFeed, VersionChange.Major);

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
                packages, NuGetSources.GlobalFeed, VersionChange.Major);

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task TestEmptyList()
        {
            var lookup = BuildBulkPackageLookup();

            var results = await lookup.FindVersionUpdates(
                Enumerable.Empty<PackageIdentity>(), NuGetSources.GlobalFeed, VersionChange.Major);

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
                packages, NuGetSources.GlobalFeed, VersionChange.Major);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results, Does.ContainKey("Moq"));
            Assert.That(results, Does.ContainKey("Newtonsoft.Json"));
        }

        private static BulkPackageLookup BuildBulkPackageLookup()
        {
            var nuKeeperLogger = Substitute.For<INuKeeperLogger>();
            var lookup = new ApiPackageLookup(new PackageVersionsLookup(
                Substitute.For<ILogger>(), nuKeeperLogger));
            return new BulkPackageLookup(lookup, new PackageLookupResultReporter(nuKeeperLogger));
        }

        private PackageIdentity Current(string packageId)
        {
            return new PackageIdentity(packageId, new NuGetVersion(1, 2, 3));
        }
    }
}
