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
        public async Task UnknownPackageName_ShouldNotReturnResult()
        {
            IApiPackageLookup lookup = new ApiPackageLookup(new NullNuGetLogger(), BuildDefaultSettings());

            var package = await lookup.LookupLatest(Guid.NewGuid().ToString());

            Assert.That(package, Is.Null);
        }

        [Test]
        public async Task WellKnownPackageName_ShouldReturnResult()
        {
            IApiPackageLookup lookup = new ApiPackageLookup(new NullNuGetLogger(), BuildDefaultSettings());

            var package = await lookup.LookupLatest("Newtonsoft.Json");

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Identity, Is.Not.Null);
            Assert.That(package.Identity.Id, Is.EqualTo("Newtonsoft.Json"));
        }

        [Test]
        public async Task AmbigousPackageName_ShouldReturnCorrectResult()
        {
            IApiPackageLookup lookup = new ApiPackageLookup(new NullNuGetLogger(), BuildDefaultSettings());

            var package = await lookup.LookupLatest("AWSSDK");

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Identity, Is.Not.Null);
            Assert.That(package.Identity.Id, Is.EqualTo("AWSSDK"));
        }

        private static Settings BuildDefaultSettings()
        {
            return new Settings((RepositoryModeSettings)null){NuGetSources = new []{ "https://api.nuget.org/v3/index.json" } };
        }
    }
}
