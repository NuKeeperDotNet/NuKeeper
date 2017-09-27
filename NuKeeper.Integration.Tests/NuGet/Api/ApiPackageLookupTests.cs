using System;
using System.Threading.Tasks;
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

            var package = await lookup.LookupLatest("AWSSDK");

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Identity, Is.Not.Null);
            Assert.That(package.Identity.Id, Is.EqualTo("AWSSDK"));
        }

        [Test]
        public async Task UnknownPackageName_ShouldNotReturnResult()
        {
            var lookup = BuildPackageLookup();

            var package = await lookup.LookupLatest(Guid.NewGuid().ToString());

            Assert.That(package, Is.Null);
        }

        [Test]
        public async Task WellKnownPackageName_ShouldReturnResult()
        {
            var lookup = BuildPackageLookup();

            var package = await lookup.LookupLatest("Newtonsoft.Json");

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Identity, Is.Not.Null);
            Assert.That(package.Identity.Id, Is.EqualTo("Newtonsoft.Json"));
        }

        private static Settings BuildDefaultSettings()
        {
            return new Settings((RepositoryModeSettings) null)
            {
                NuGetSources = new[] {"https://api.nuget.org/v3/index.json"}
            };
        }

        private IApiPackageLookup BuildPackageLookup()
        {
            return new ApiPackageLookup(
                new PackageVersionsLookup(new NullNuGetLogger(), BuildDefaultSettings()));
        }
    }
}