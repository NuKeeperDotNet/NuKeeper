using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Engine.Packages;
using NuKeeper.Git;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update.Process;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class RepositoryUpdaterTests
    {
        [Test]
        public async Task WhenThereAreNoUpdates_CountIsZero()
        {
            var repoUpdater = MakeRepositoryUpdater();

            var git = Substitute.For<IGitDriver>();
            var repo = new RepositoryData(
                new ForkData(new Uri("http://foo.com"), "me", "test"),
                new ForkData(new Uri("http://foo.com"), "me", "test"));

            var count = await repoUpdater.Run(git, repo);

            Assert.That(count, Is.EqualTo(0));
        }

        private static IRepositoryUpdater MakeRepositoryUpdater()
        {
            var sources = Substitute.For<INuGetSourcesReader>();
            var updateFinder = Substitute.For<IUpdateFinder>();
            var updateSelection = Substitute.For<IPackageUpdateSelection>();
            var packageUpdater = Substitute.For<IPackageUpdater>();
            var fileRestore = Substitute.For<IFileRestoreCommand>();
            var reporter = Substitute.For<IAvailableUpdatesReporter>();

            updateFinder.FindPackageUpdateSets(
                    Arg.Any<IFolder>(),
                    Arg.Any<NuGetSources>(),
                    Arg.Any<VersionChange>())
                .Returns(new List<PackageUpdateSet>());

            var repoUpdater = new RepositoryUpdater(
                sources, updateFinder, updateSelection, packageUpdater,
                new NullNuKeeperLogger(), new SolutionsRestore(fileRestore),
                reporter, new UserSettings());

            return repoUpdater;
        }
    }
}
