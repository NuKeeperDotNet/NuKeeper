using System;
using System.IO;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.EndToEndTests
{
    public class FinderTests
    {
        [Test]
        public async Task TestMissingFolder()
        {
            var logger = new TestLogger();
            var finder = BuildUpdateFinder(logger);

            Assert.That(finder, Is.Not.Null);

            var updates = await finder.FindPackageUpdateSets(
                FolderForPath(".{sep}NoSuchPath"),
                NuGetSources.GlobalFeed, VersionChange.Major, UsePrerelease.Never);

            Assert.That(updates, Is.Not.Null);
            Assert.That(updates, Is.Empty);

            Assert.That(logger.ReceivedMessage("Found 0 possible updates"));
        }

        [Test]
        public async Task TestSimpleFolder()
        {
            var logger = new TestLogger();
            var finder = BuildUpdateFinder(logger);

            Assert.That(finder, Is.Not.Null);

            var updates = await finder.FindPackageUpdateSets(
                FolderForPath(".{sep}Data{sep}newStyleCsProj"),
                NuGetSources.GlobalFeed, VersionChange.Major, UsePrerelease.Never);

            Assert.That(updates, Is.Not.Null);
            Assert.That(updates, Is.Not.Empty);
            Assert.That(logger.ReceivedMessage("Found 1 possible updates"));
            Assert.That(logger.ReceivedMessage("Newtonsoft.Json"));
        }

        private Folder FolderForPath(string path)
        {
            path = path.Replace("{sep}", "" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

            var logger = new TestLogger();
            return new Folder(logger, new DirectoryInfo(path));
        }

        private static UpdateFinder BuildUpdateFinder(INuKeeperLogger logger)
        {
            var nugetLogger = new NuGetLogger(logger);

            var scanner = new RepositoryScanner(
                new ProjectFileReader(logger),
                new PackagesFileReader(logger),
                new NuspecFileReader(logger),
                new DirectoryBuildTargetsReader(logger),
                new NullExclusions());

            var lookup =
                new PackageUpdatesLookup(
                    new BulkPackageLookup(new ApiPackageLookup(new PackageVersionsLookup(nugetLogger, logger)),
                        new PackageLookupResultReporter(logger)));

            var finder = new UpdateFinder(scanner, lookup, logger);
            return finder;
        }
    }
}
