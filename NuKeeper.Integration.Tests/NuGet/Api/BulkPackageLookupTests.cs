using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.NuGet.Api;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.Nuget.Api
{
    [TestFixture]
    public class BulkPackageLookupTests
    {
        [Test]
        public async Task TestEmptyList()
        {
            var lookup = BuildBulkPackageLookup();

            var results = await lookup.LatestVersions(Enumerable.Empty<string>());

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task CanFindUpdateForOneWellKnownPackage()
        {
            var packages = new List<string> { "Moq" };

            var lookup = BuildBulkPackageLookup();

            var results = await lookup.LatestVersions(packages);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results, Does.ContainKey("Moq"));
        }

        [Test]
        public async Task CanFindUpdateForTwoWellKnownPackages()
        {
            var packages = new List<string>
            {
                "Moq",
                "Newtonsoft.Json"
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
            var packages = new List<string> { Guid.NewGuid().ToString() };

            var lookup = BuildBulkPackageLookup();

            var results = await lookup.LatestVersions(packages);

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task ValidPackagesWorkDespiteInvalidPackages()
        {
            var packages = new List<string>
            {
                "Moq",
                Guid.NewGuid().ToString(),
                "Newtonsoft.Json",
                Guid.NewGuid().ToString()
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
            return new BulkPackageLookup(new ApiPackageLookup(new NullNuGetLogger()), new NullNuKeeperLogger());
        }
    }
}