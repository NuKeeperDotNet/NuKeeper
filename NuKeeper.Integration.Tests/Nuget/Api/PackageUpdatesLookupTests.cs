using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.NuGet.Api;
using NuKeeper.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.Nuget.Api
{
    [TestFixture]
    public class PackageUpdatesLookupTests
    {
        [Test]
        public async Task TestEmptyList()
        {
            var lookup = new PackageUpdatesLookup(new ApiPackageLookup());

            var results = await lookup.FindUpdatesForPackages(Enumerable.Empty<PackageInProject>());

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task CanFindUpdateForOneWellKnownPackage()
        {
            var packages = new List<PackageInProject>
                {
                    new PackageInProject(MoqPackage(), TempPath())
                };

            var lookup = new PackageUpdatesLookup(new ApiPackageLookup());

            var results = await lookup.FindUpdatesForPackages(packages);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].PackageId, Is.EqualTo("Moq"));
        }

        [Test]
        public async Task CanFindUpdateForTwoWellKnownPackages()
        {
            var packages = new List<PackageInProject>
                {
                    new PackageInProject(MoqPackage(), TempPath()),
                    new PackageInProject(NewtonsoftJsonPackage(), TempPath())
                };

            var lookup = new PackageUpdatesLookup(new ApiPackageLookup());

            var results = await lookup.FindUpdatesForPackages(packages);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results[0].PackageId, Is.EqualTo("Moq"));
            Assert.That(results[1].PackageId, Is.EqualTo("Newtonsoft.Json"));
        }

        [Test]
        public async Task IsNotBrokenByInvalidPackages()
        {
            var packages = new List<PackageInProject>
                {
                    new PackageInProject(MoqPackage(), TempPath()),
                    new PackageInProject(InvalidPackage(), TempPath()),
                    new PackageInProject(NewtonsoftJsonPackage(), TempPath()),
                    new PackageInProject(InvalidPackage(), TempPath())
                };

            var lookup = new PackageUpdatesLookup(new ApiPackageLookup());

            var results = await lookup.FindUpdatesForPackages(packages);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(2));
        }

        private PackagePath TempPath()
        {
            return new PackagePath("c:\\temp\\somewhere", "src\\packages.config",
                PackageReferenceType.PackagesConfig);
        }

        private PackageIdentity NewtonsoftJsonPackage()
        {
            return new PackageIdentity("Newtonsoft.Json", new NuGetVersion("2.0.0"));
        }

        private PackageIdentity MoqPackage()
        {
            return new PackageIdentity("Moq", new NuGetVersion("1.0.0"));
        }

        private PackageIdentity InvalidPackage()
        {
            return new PackageIdentity(Guid.NewGuid().ToString(), new NuGetVersion("3.0.0"));
        }
    }
}