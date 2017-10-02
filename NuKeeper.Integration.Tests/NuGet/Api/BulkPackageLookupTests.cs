using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Configuration;
using NuKeeper.NuGet.Api;
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

            var results = await lookup.LatestVersions(packages);

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

            var results = await lookup.LatestVersions(packages);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results, Does.ContainKey("Moq"));
            Assert.That(results, Does.ContainKey("Newtonsoft.Json"));
        }

        [Test]
        public async Task InvalidPackageIsIgnored()
        {
            var packages = new List<PackageIdentity>
            {
                Current(Guid.NewGuid().ToString())
            };

            var lookup = BuildBulkPackageLookup();

            var results = await lookup.LatestVersions(packages);

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task TestEmptyList()
        {
            var lookup = BuildBulkPackageLookup();

            var results = await lookup.LatestVersions(Enumerable.Empty<PackageIdentity>());

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

            var results = await lookup.LatestVersions(packages);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results, Does.ContainKey("Moq"));
            Assert.That(results, Does.ContainKey("Newtonsoft.Json"));
        }

        private static BulkPackageLookup BuildBulkPackageLookup()
        {
            var lookup = new ApiPackageLookup(new PackageVersionsLookup(new NullNuGetLogger(), BuildDefaultSettings()));
            return new BulkPackageLookup(lookup, new NullNuKeeperLogger());
        }

        private static Settings BuildDefaultSettings()
        {
            return new Settings((RepositoryModeSettings) null)
            {
                NuGetSources = new[] {"https://api.nuget.org/v3/index.json"}
            };
        }

        private PackageIdentity Current(string packageId)
        {
            return new PackageIdentity(packageId, new NuGetVersion(1, 2, 3));
        }
    }
}