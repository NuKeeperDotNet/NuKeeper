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
            Assert.That(package.Highest, Is.Not.Null);
            Assert.That(package.Match, Is.Not.Null);
            Assert.That(package.Match.Identity, Is.Not.Null);
            Assert.That(package.Match.Identity.Id, Is.EqualTo("AWSSDK"));
        }

        [Test]
        public async Task UnknownPackageName_ShouldNotReturnResult()
        {
            var lookup = BuildPackageLookup();

            var package = await lookup.FindVersionUpdate(
                Current(Guid.NewGuid().ToString()), 
                VersionChange.Major);

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Highest, Is.Null);
            Assert.That(package.Match, Is.Null);
        }

        [Test]
        public async Task WellKnownPackageName_ShouldReturnResult()
        {
            var lookup = BuildPackageLookup();

            var package = await lookup.FindVersionUpdate(
                Current("Newtonsoft.Json"), 
                VersionChange.Major);

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Highest, Is.Not.Null);
            Assert.That(package.Match, Is.Not.Null);
            Assert.That(package.Match.Identity, Is.Not.Null);
            Assert.That(package.Match.Identity.Id, Is.EqualTo("Newtonsoft.Json"));
        }

        private static UserPreferences BuildDefaultSettings()
        {
            return new UserPreferences
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