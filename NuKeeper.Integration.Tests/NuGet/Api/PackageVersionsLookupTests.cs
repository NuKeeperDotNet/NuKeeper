using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Integration.Tests.NuGet.Api;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.Nuget.Api
{
    [TestFixture]
    public class PackageVersionsLookupTests
    {
        [Test]
        public async Task WellKnownPackageName_ShouldReturnResultsList()
        {
            var lookup = BuildPackageLookup();

            var packages = await lookup.Lookup("Newtonsoft.Json");

            Assert.That(packages, Is.Not.Null);

            var packageList = packages.ToList();
            Assert.That(packageList, Is.Not.Empty);
            Assert.That(packageList.Count, Is.GreaterThan(1));
        }

        [Test]
        public async Task WellKnownPackageName_ShouldReturnPopulatedResults()
        {
            var lookup = BuildPackageLookup();

            var packages = await lookup.Lookup("Newtonsoft.Json");

            Assert.That(packages, Is.Not.Null);

            var packageList = packages.ToList();
            var latest = packageList
                .OrderByDescending(p => p.Identity.Version)
                .FirstOrDefault();

            Assert.That(latest, Is.Not.Null);
            Assert.That(latest.Identity, Is.Not.Null);
            Assert.That(latest.Identity.Version, Is.Not.Null);

            Assert.That(latest.Identity.Id, Is.EqualTo("Newtonsoft.Json"));
            Assert.That(latest.Identity.Version.Major, Is.GreaterThan(1));
            Assert.That(latest.Published.HasValue, Is.True);
        }

        private IPackageVersionsLookup BuildPackageLookup()
        {
            return new PackageVersionsLookup(new NullNuGetLogger(), BuildDefaultSettings());
        }

        private static IEnumerable<string> BuildDefaultSettings()
        {
            return new [] { "https://api.nuget.org/v3/index.json" };
        }
    }
}
