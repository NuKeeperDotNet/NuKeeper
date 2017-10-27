using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Integration.Tests.NuGet.Api;
using NuKeeper.NuGet.Api;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.Nuget.Api
{
    [TestFixture]
    public class PackageVersionsLookupTests
    {
        [Test]
        public async Task WellKnownPackageName_ShouldReturnResults()
        {
            var lookup = BuildPackageLookup();

            var packages = await lookup.Lookup("Newtonsoft.Json");

            Assert.That(packages, Is.Not.Null);

            var packageList = packages.ToList();
            Assert.That(packageList, Is.Not.Empty);
            Assert.That(packageList[0].Identity.Id, Is.EqualTo("Newtonsoft.Json"));
        }

        private IPackageVersionsLookup BuildPackageLookup()
        {
            return new PackageVersionsLookup(new NullNuGetLogger(), BuildDefaultSettings());
        }

        private static UserPreferences BuildDefaultSettings()
        {
            return new UserPreferences
            {
                NuGetSources = new[] { "https://api.nuget.org/v3/index.json" }
            };
        }
    }
}
