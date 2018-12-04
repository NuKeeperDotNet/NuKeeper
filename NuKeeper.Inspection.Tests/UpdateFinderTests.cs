using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;
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

            ReturnsUpdateSetForEachPackage(updater);

            var finder = new UpdateFinder(scanner, updater, logger);

            var results = await finder.FindPackageUpdateSets(
                folder, NuGetSources.GlobalFeed, VersionChange.Major, UsePrerelease.FromPrerelease);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(0));

            logger
                .DidNotReceive()
                .Error(Arg.Any<string>(), Arg.Any<Exception>());
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

            ReturnsUpdateSetForEachPackage(updater);

            var finder = new UpdateFinder(scanner, updater, logger);

            var results = await finder.FindPackageUpdateSets(
                folder, NuGetSources.GlobalFeed, VersionChange.Major, UsePrerelease.FromPrerelease);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().SelectedId, Is.EqualTo("somePackage"));

            logger
                .DidNotReceive()
                .Error(Arg.Any<string>(), Arg.Any<Exception>());
        }

        [Test]
        public async Task FindSkipsMetapackageResult()
        {
            var scanner = Substitute.For<IRepositoryScanner>();
            var updater = Substitute.For<IPackageUpdatesLookup>();
            var logger = Substitute.For<INuKeeperLogger>();
            var folder = Substitute.For<IFolder>();

            var pip = BuildPackageInProject("somePackage");
            var aspnetCore = BuildPackageInProject("Microsoft.AspNetCore.App");

            scanner.FindAllNuGetPackages(Arg.Any<IFolder>())
                .Returns(new List<PackageInProject> { pip, aspnetCore });

            ReturnsUpdateSetForEachPackage(updater);

            var finder = new UpdateFinder(scanner, updater, logger);

            var results = await finder.FindPackageUpdateSets(
                folder, NuGetSources.GlobalFeed, VersionChange.Major, UsePrerelease.FromPrerelease);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().SelectedId, Is.EqualTo("somePackage"));

            logger
                .Received(1)
                .Error(Arg.Is<string>(
                    s => s.StartsWith("Metapackage 'Microsoft.AspNetCore.App'", StringComparison.OrdinalIgnoreCase)));
        }

        [Test]
        public async Task FindSkipsBothMetapackageResult()
        {
            var scanner = Substitute.For<IRepositoryScanner>();
            var updater = Substitute.For<IPackageUpdatesLookup>();
            var logger = Substitute.For<INuKeeperLogger>();
            var folder = Substitute.For<IFolder>();

            var aspnetCoreAll = BuildPackageInProject("Microsoft.AspNetCore.All");
            var pip = BuildPackageInProject("somePackage");
            var aspnetCoreApp = BuildPackageInProject("Microsoft.AspNetCore.App");

            scanner.FindAllNuGetPackages(Arg.Any<IFolder>())
                .Returns(new List<PackageInProject> { aspnetCoreAll, pip, aspnetCoreApp });

            ReturnsUpdateSetForEachPackage(updater);

            var finder = new UpdateFinder(scanner, updater, logger);

            var results = await finder.FindPackageUpdateSets(
                folder, NuGetSources.GlobalFeed, VersionChange.Major, UsePrerelease.FromPrerelease);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().SelectedId, Is.EqualTo("somePackage"));

            logger
                .Received(1)
                .Error(Arg.Is<string>(s => s.StartsWith("Metapackage 'Microsoft.AspNetCore.App'", StringComparison.OrdinalIgnoreCase)));
            logger
                .Received(1)
                .Error(Arg.Is<string>(s => s.StartsWith("Metapackage 'Microsoft.AspNetCore.All'", StringComparison.OrdinalIgnoreCase)));
        }

        private void ReturnsUpdateSetForEachPackage(IPackageUpdatesLookup updater)
        {
            updater.FindUpdatesForPackages(
                    Arg.Any<IReadOnlyCollection<PackageInProject>>(),
                    Arg.Any<NuGetSources>(),
                    Arg.Any<VersionChange>(),
                    Arg.Any<UsePrerelease>())
                .Returns(a => a.ArgAt<IReadOnlyCollection<PackageInProject>>(0)
                    .Select(BuildPackageUpdateSet)
                    .ToList());
        }

        private static PackageInProject BuildPackageInProject(string packageName)
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
