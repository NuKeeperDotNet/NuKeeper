using System;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Configuration;
using NuKeeper.NuGet.Api;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.NuGet.Api
{
    [TestFixture]
    public class ApiPackageLookupTests
    {
        [Test]
        public async Task AmbigousPackageName_ShouldReturnCorrectResult()
        {
            var lookup = BuildPackageLookup();

            var package = await lookup.FindVersionUpdate(Current("AWSSDK"), VersionChange.Major);

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Major, Is.Not.Null);

            var selected = package.Selected();

            Assert.That(selected, Is.Not.Null);
            Assert.That(selected.Identity, Is.Not.Null);
            Assert.That(selected.Identity.Id, Is.EqualTo("AWSSDK"));
        }

        [Test]
        public async Task UnknownPackageName_ShouldNotReturnResult()
        {
            var lookup = BuildPackageLookup();

            var package = await lookup.FindVersionUpdate(
                Current(Guid.NewGuid().ToString()), 
                VersionChange.Major);

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Major, Is.Null);
            Assert.That(package.Selected(), Is.Null);
        }

        [Test]
        public async Task WellKnownPackageName_ShouldReturnResult()
        {
            var lookup = BuildPackageLookup();

            var package = await lookup.FindVersionUpdate(
                Current("Newtonsoft.Json"), 
                VersionChange.Major);

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Major, Is.Not.Null);

            var selected = package.Selected();

            Assert.That(selected, Is.Not.Null);
            Assert.That(selected.Identity, Is.Not.Null);
            Assert.That(selected.Identity.Id, Is.EqualTo("Newtonsoft.Json"));
        }

        [Test]
        public async Task MinorUpdateToWellKnownPackage()
        {
            var lookup = BuildPackageLookup();

            // when we ask for updates for newtonsoft 8.0.1
            // we know that there is a later patch (8.0.3)
            // and later major versions 9.0.1, 10.0.3 etc
            var package = await lookup.FindVersionUpdate(
                new PackageIdentity("Newtonsoft.Json", new NuGetVersion(8, 0, 1)),
                VersionChange.Minor);

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Major, Is.Not.Null);
            Assert.That(package.Minor, Is.Not.Null);
            Assert.That(package.Patch, Is.Not.Null);
            Assert.That(package.Selected(), Is.Not.Null);

            Assert.That(package.Major, Is.Not.EqualTo(package.Selected()));
            Assert.That(package.Major, Is.Not.EqualTo(package.Patch));

            Assert.That(package.Minor.Identity.Version.Major, Is.EqualTo(8));
            Assert.That(package.Minor.Identity.Version.Patch, Is.GreaterThan(1));

            Assert.That(package.Patch.Identity.Version.Major, Is.EqualTo(8));
            Assert.That(package.Patch.Identity.Version.Patch, Is.GreaterThan(1));

            Assert.That(package.Minor.Identity.Version.Major, Is.EqualTo(8));
            Assert.That(package.Major.Identity.Version.Major, Is.GreaterThan(8));
        }

        private static UserSettings BuildDefaultSettings()
        {
            return new UserSettings
            {
                NuGetSources = new[] {"https://api.nuget.org/v3/index.json"}
            };
        }

        private IApiPackageLookup BuildPackageLookup()
        {
            return new ApiPackageLookup(
                new PackageVersionsLookup(new NullNuGetLogger(), BuildDefaultSettings()));
        }

        private PackageIdentity Current(string packageId)
        {
            return new PackageIdentity(packageId, new NuGetVersion(1,2,3));
        }
    }
}
