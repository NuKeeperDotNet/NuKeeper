using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Common;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Sources;
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

            var packages = await lookup.Lookup("Newtonsoft.Json", false, NuGetSources.GlobalFeed);

            Assert.That(packages, Is.Not.Null);

            var packageList = packages.ToList();
            Assert.That(packageList, Is.Not.Empty);
            Assert.That(packageList.Count, Is.GreaterThan(1));
        }

        [Test]
        public async Task WellKnownPackageName_ShouldReturnPopulatedResults()
        {
            var lookup = BuildPackageLookup();

            var packages = await lookup.Lookup("Newtonsoft.Json", false, NuGetSources.GlobalFeed);

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
            Assert.That(latest.Identity.Version.IsPrerelease, Is.False);
        }

        [Test]
        public async Task CanGetPreReleases()
        {
            var lookup = BuildPackageLookup();

            var packages = await lookup.Lookup("Moq", true, NuGetSources.GlobalFeed);

            Assert.That(packages, Is.Not.Null);

            var betas = packages
                .Where(p => p.Identity.Version.IsPrerelease)
                .OrderByDescending(p => p.Identity.Version)
                .ToList();

            Assert.That(betas, Is.Not.Null);
            Assert.That(betas, Is.Not.Empty);

            var beta = betas.FirstOrDefault();

            Assert.That(beta, Is.Not.Null);
            Assert.That(beta.Identity, Is.Not.Null);
            Assert.That(beta.Identity.Version, Is.Not.Null);

            Assert.That(beta.Identity.Id, Is.EqualTo("Moq"));
            Assert.That(beta.Identity.Version.IsPrerelease, Is.True);
        }

        [Test]
        public async Task PackageShouldHaveDependencies()
        {
            var lookup = BuildPackageLookup();

            var packages = await lookup.Lookup("Moq", false, NuGetSources.GlobalFeed);

            Assert.That(packages, Is.Not.Null);

            var packageList = packages.ToList();
            var latest = packageList
                .OrderByDescending(p => p.Identity.Version)
                .FirstOrDefault();

            Assert.That(latest, Is.Not.Null);
            Assert.That(latest.Dependencies, Is.Not.Null);
            Assert.That(latest.Dependencies, Is.Not.Empty);
        }

        [Test]
        public async Task CanBeCalledTwice()
        {
            var lookup = BuildPackageLookup();
            var packages1 = await lookup.Lookup("Newtonsoft.Json", false, NuGetSources.GlobalFeed);
            Assert.That(packages1, Is.Not.Null);

            var packages2 = await lookup.Lookup("Moq", false, NuGetSources.GlobalFeed);
            Assert.That(packages2, Is.Not.Null);
        }

        private IPackageVersionsLookup BuildPackageLookup()
        {
            return new PackageVersionsLookup(
                Substitute.For<ILogger>(), Substitute.For<INuKeeperLogger>());
        }
    }
}
