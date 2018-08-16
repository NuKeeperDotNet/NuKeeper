using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Engine.Packages;
using NuKeeper.Git;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update.Process;
using NuKeeper.Update.Selection;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class RepositoryUpdaterTests
    {
        [Test]
        public async Task WhenThereAreNoUpdates_CountIsZero()
        {
            var packageUpdater = Substitute.For<IPackageUpdater>();
            var updateSelection = Substitute.For<IPackageUpdateSelection>();
            UpdateSelectionAll(updateSelection);

            var repoUpdater = MakeRepositoryUpdater(
                packageUpdater, updateSelection,
                new List<PackageUpdateSet>());

            var git = Substitute.For<IGitDriver>();
            var repo = MakeRepositoryData();

            var count = await repoUpdater.Run(git, repo);

            Assert.That(count, Is.EqualTo(0));
            await AssertDidNotReceiveMakeUpdate(packageUpdater);
        }

        [Test]
        public async Task WhenThereIsAnUpdate_CountIsOne()
        {
            var packageUpdater = Substitute.For<IPackageUpdater>();
            var updateSelection = Substitute.For<IPackageUpdateSelection>();
            UpdateSelectionAll(updateSelection);

            var repoUpdater = MakeRepositoryUpdater(
                packageUpdater, updateSelection,
                new List<PackageUpdateSet>
                {
                    UpdateSet()
                });

            var git = Substitute.For<IGitDriver>();
            var repo = MakeRepositoryData();

            var count = await repoUpdater.Run(git, repo);

            Assert.That(count, Is.EqualTo(1));
            await AssertReceivedMakeUpdate(packageUpdater,1);
        }

        [Test]
        public async Task WhenThereAreTwoUpdates_CountIsTwo()
        {
            var packageUpdater = Substitute.For<IPackageUpdater>();
            var updateSelection = Substitute.For<IPackageUpdateSelection>();
            UpdateSelectionAll(updateSelection);

            var twoUpdates = new List<PackageUpdateSet>
            {
                UpdateSet(),
                UpdateSet()
            };

            var repoUpdater = MakeRepositoryUpdater(
                packageUpdater, updateSelection,
                twoUpdates);

            var git = Substitute.For<IGitDriver>();
            var repo = MakeRepositoryData();

            var count = await repoUpdater.Run(git, repo);

            Assert.That(count, Is.EqualTo(2));
            await AssertReceivedMakeUpdate(packageUpdater, 2);
        }

        [Test]
        public async Task WhenUpdatesAreFilteredOut_CountIsZero()
        {
            var packageUpdater = Substitute.For<IPackageUpdater>();
            var updateSelection = Substitute.For<IPackageUpdateSelection>();
            UpdateSelectionNone(updateSelection);

            var twoUpdates = new List<PackageUpdateSet>
            {
                UpdateSet(),
                UpdateSet()
            };

            var repoUpdater = MakeRepositoryUpdater(
                packageUpdater, updateSelection,
                twoUpdates);

            var git = Substitute.For<IGitDriver>();
            var repo = MakeRepositoryData();

            var count = await repoUpdater.Run(git, repo);

            Assert.That(count, Is.EqualTo(0));
            await AssertDidNotReceiveMakeUpdate(packageUpdater);
        }

        [Test]
        public async Task WhenReportOnly_CountIsZero()
        {
            var packageUpdater = Substitute.For<IPackageUpdater>();
            var updateSelection = Substitute.For<IPackageUpdateSelection>();
            UpdateSelectionAll(updateSelection);

            var twoUpdates = new List<PackageUpdateSet>
                {
                    UpdateSet(),
                    UpdateSet()
                };

            var repoUpdater = MakeRepositoryUpdater(
                packageUpdater, updateSelection,
                twoUpdates,
                ReportMode.ReportOnly);

            var git = Substitute.For<IGitDriver>();
            var repo = MakeRepositoryData();

            var count = await repoUpdater.Run(git, repo);

            Assert.That(count, Is.EqualTo(0));
            await AssertDidNotReceiveMakeUpdate(packageUpdater);
        }

        private async Task AssertReceivedMakeUpdate(
            IPackageUpdater packageUpdater,
            int count)
        {
            await packageUpdater.Received(count)
                .MakeUpdatePullRequest(
                    Arg.Any<IGitDriver>(),
                    Arg.Any<PackageUpdateSet>(),
                    Arg.Any<NuGetSources>(),
                    Arg.Any<RepositoryData>());
        }

        private async Task AssertDidNotReceiveMakeUpdate(
            IPackageUpdater packageUpdater)
        {
            await packageUpdater.DidNotReceiveWithAnyArgs()
                .MakeUpdatePullRequest(
                    Arg.Any<IGitDriver>(),
                    Arg.Any<PackageUpdateSet>(),
                    Arg.Any<NuGetSources>(),
                    Arg.Any<RepositoryData>());
        }

        private void UpdateSelectionAll(IPackageUpdateSelection updateSelection)
        {
            updateSelection.SelectTargets(
                    Arg.Any<ForkData>(),
                    Arg.Any<IReadOnlyCollection<PackageUpdateSet>>(),
                    Arg.Any<FilterSettings>())
                .Returns(c => c.ArgAt<IReadOnlyCollection<PackageUpdateSet>>(1));
        }

        private void UpdateSelectionNone(IPackageUpdateSelection updateSelection)
        {
            updateSelection.SelectTargets(
                    Arg.Any<ForkData>(),
                    Arg.Any<IReadOnlyCollection<PackageUpdateSet>>(),
                    Arg.Any<FilterSettings>())
                .Returns(new List<PackageUpdateSet>());
        }

        private IRepositoryUpdater MakeRepositoryUpdater(
            IPackageUpdater packageUpdater,
            IPackageUpdateSelection updateSelection,
            List<PackageUpdateSet> updates,
            ReportMode reportMode = ReportMode.Off)
        {
            var sources = Substitute.For<INuGetSourcesReader>();
            var updateFinder = Substitute.For<IUpdateFinder>();
            var fileRestore = Substitute.For<IFileRestoreCommand>();
            var reporter = Substitute.For<IAvailableUpdatesReporter>();

            updateFinder.FindPackageUpdateSets(
                    Arg.Any<IFolder>(),
                    Arg.Any<NuGetSources>(),
                    Arg.Any<VersionChange>())
                .Returns(updates);

            packageUpdater.MakeUpdatePullRequest(
                Arg.Any<IGitDriver>(),
                Arg.Any<PackageUpdateSet>(),
                Arg.Any<NuGetSources>(),
                Arg.Any<RepositoryData>())
                .Returns(true);

            var settings = new SettingsContainer
            {
                UserSettings = new UserSettings
                {
                    ReportMode = reportMode
                }
            };

            var repoUpdater = new RepositoryUpdater(
                sources, updateFinder, updateSelection, packageUpdater,
                Substitute.For<INuKeeperLogger>(), new SolutionsRestore(fileRestore),
                reporter, settings);

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
            var latest = new PackageSearchMedatadata(fooPackage, new PackageSource("https://somewhere"), publishedDate, null);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, packages);
        }
    }
}
