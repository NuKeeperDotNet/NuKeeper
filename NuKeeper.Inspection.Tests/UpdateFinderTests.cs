using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests
{
    [TestFixture]
    public class UpdateFinderTests
    {
        [Test]
        public async Task FindWithoutResults()
        {
            var scanner = Substitute.For<IRepositoryScanner>();
            var updater = Substitute.For<IPackageUpdatesLookup>();
            var logger = Substitute.For<INuKeeperLogger>();
            var folder = Substitute.For<IFolder>();

            var finder = new UpdateFinder(scanner, updater, logger);

            var results = await finder.FindPackageUpdateSets(
                folder, NuGetSources.GlobalFeed, VersionChange.Major);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task FindWithOneResult()
        {
            var scanner = Substitute.For<IRepositoryScanner>();
            var updater = Substitute.For<IPackageUpdatesLookup>();
            var logger = Substitute.For<INuKeeperLogger>();
            var folder = Substitute.For<IFolder>();

            var pip = BuildPackageInProject("somePackage");

            scanner.FindAllNuGetPackages(Arg.Any<IFolder>())
                .Returns(new List<PackageInProject> { pip });

            updater.FindUpdatesForPackages(
                    Arg.Any<IReadOnlyCollection<PackageInProject>>(),
                    Arg.Any<NuGetSources>(),
                    Arg.Any<VersionChange>())
                .Returns(new List<PackageUpdateSet>{ BuildPackageUpdateSet(pip) } );

            var finder = new UpdateFinder(scanner, updater, logger);

            var results = await finder.FindPackageUpdateSets(
                folder, NuGetSources.GlobalFeed, VersionChange.Major);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
        }

        private PackageInProject BuildPackageInProject(string packageName)
        {
            var path = new PackagePath("c:\\temp", "folder\\src\\project1\\packages.config",
                PackageReferenceType.PackagesConfig);
            return new PackageInProject(packageName, "1.1.0", path);
        }

        private PackageUpdateSet BuildPackageUpdateSet(PackageInProject pip)
        {
            var package = new PackageIdentity(pip.Id, new NuGetVersion("1.4.5"));
            var latest = new PackageSearchMedatadata(package, new PackageSource("http://none"), null, null);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);

            return new PackageUpdateSet(updates, new List<PackageInProject> {pip });
        }

    }
}
