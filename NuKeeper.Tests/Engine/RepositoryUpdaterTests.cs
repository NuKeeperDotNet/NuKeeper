using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Packaging.Core;
using NuGet.Versioning;
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
        private IPackageUpdater _packageUpdater;

        [Test]
        public async Task WhenThereAreNoUpdates_CountIsZero()
        {
            _packageUpdater = Substitute.For<IPackageUpdater>();

            var repoUpdater = MakeRepositoryUpdater(new List<PackageUpdateSet>());

            var git = Substitute.For<IGitDriver>();
            var repo = MakeRepositoryData();

            var count = await repoUpdater.Run(git, repo);

            Assert.That(count, Is.EqualTo(0));

            await _packageUpdater.DidNotReceiveWithAnyArgs()
                .MakeUpdatePullRequest(Arg.Any<IGitDriver>(),
                    Arg.Any<PackageUpdateSet>(),
                    Arg.Any<NuGetSources>(),
                    Arg.Any<RepositoryData>());
        }

        [Test]
        public async Task WhenThereIsAnUpdate_CountIsOne()
        {
            _packageUpdater = Substitute.For<IPackageUpdater>();

            var repoUpdater = MakeRepositoryUpdater(
                new List<PackageUpdateSet>
                {
                    UpdateSet()
                });

            var git = Substitute.For<IGitDriver>();
            var repo = MakeRepositoryData();

            var count = await repoUpdater.Run(git, repo);

            Assert.That(count, Is.EqualTo(1));

            await _packageUpdater.Received(1)
                .MakeUpdatePullRequest(Arg.Any<IGitDriver>(),
                    Arg.Any<PackageUpdateSet>(),
                    Arg.Any<NuGetSources>(),
                    Arg.Any<RepositoryData>());
        }

        [Test]
        public async Task WhenThereAreTwoUpdates_CountIsTwo()
        {
            _packageUpdater = Substitute.For<IPackageUpdater>();

            var repoUpdater = MakeRepositoryUpdater(
                new List<PackageUpdateSet>
                {
                    UpdateSet(),
                    UpdateSet()
                });

            var git = Substitute.For<IGitDriver>();
            var repo = MakeRepositoryData();

            var count = await repoUpdater.Run(git, repo);

            Assert.That(count, Is.EqualTo(2));

            await _packageUpdater.Received(2)
                .MakeUpdatePullRequest(Arg.Any<IGitDriver>(),
                    Arg.Any<PackageUpdateSet>(),
                    Arg.Any<NuGetSources>(),
                    Arg.Any<RepositoryData>());
        }

        private IRepositoryUpdater MakeRepositoryUpdater(
            List<PackageUpdateSet> updates)
        {
            var sources = Substitute.For<INuGetSourcesReader>();
            var updateFinder = Substitute.For<IUpdateFinder>();
            var updateSelection = Substitute.For<IPackageUpdateSelection>();
            var fileRestore = Substitute.For<IFileRestoreCommand>();
            var reporter = Substitute.For<IAvailableUpdatesReporter>();

            updateFinder.FindPackageUpdateSets(
                    Arg.Any<IFolder>(),
                    Arg.Any<NuGetSources>(),
                    Arg.Any<VersionChange>())
                .Returns(updates);

            updateSelection.SelectTargets(Arg.Any<ForkData>(), Arg.Any<IReadOnlyCollection<PackageUpdateSet>>())
                .Returns(c => c.ArgAt<IReadOnlyCollection<PackageUpdateSet>>(1));

            _packageUpdater.MakeUpdatePullRequest(
                Arg.Any<IGitDriver>(),
                Arg.Any<PackageUpdateSet>(),
                Arg.Any<NuGetSources>(),
                Arg.Any<RepositoryData>())
                .Returns(true);

            var repoUpdater = new RepositoryUpdater(
                sources, updateFinder, updateSelection, _packageUpdater,
                new NullNuKeeperLogger(), new SolutionsRestore(fileRestore),
                reporter, new UserSettings());

            return repoUpdater;
        }

        private static RepositoryData MakeRepositoryData()
        {
            return new RepositoryData(
                new ForkData(new Uri("http://foo.com"), "me", "test"),
                new ForkData(new Uri("http://foo.com"), "me", "test"));
        }

        private static PackageUpdateSet UpdateSet()
        {
            PackageIdentity fooPackage = new PackageIdentity("foo", new NuGetVersion(1,2,3));
            var packages = new[]
            {
                new PackageInProject(fooPackage, new PackagePath("c:\\foo", "bar", PackageReferenceType.PackagesConfig))
            };

            var publishedDate = new DateTimeOffset(2018, 2, 19, 11, 12, 7, TimeSpan.Zero);
            var latest = new PackageSearchMedatadata(fooPackage, NuGetSources.GlobalFeedUrl, publishedDate, null);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, packages);
        }
    }
}
