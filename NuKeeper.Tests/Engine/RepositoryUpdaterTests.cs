using NSubstitute;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Engine;
using NuKeeper.Engine.Packages;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update;
using NuKeeper.Update.Process;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.Inspections.Files;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class RepositoryUpdaterTests
    {
        [Test]
        public async Task WhenThereAreNoUpdates_CountIsZero()
        {
            var updateSelection = Substitute.For<IPackageUpdateSelection>();
            UpdateSelectionAll(updateSelection);

            var (repoUpdater, packageUpdater) = MakeRepositoryUpdater(
                updateSelection,
                new List<PackageUpdateSet>());

            var git = Substitute.For<IGitDriver>();
            var repo = MakeRepositoryData();

            var count = await repoUpdater.Run(git, repo, MakeSettings());

            Assert.That(count, Is.EqualTo(0));
            await AssertDidNotReceiveMakeUpdate(packageUpdater);
        }

        [Test]
        public async Task WhenThereIsAnUpdate_CountIsOne()
        {
            var updateSelection = Substitute.For<IPackageUpdateSelection>();
            UpdateSelectionAll(updateSelection);

            var updates = PackageUpdates.UpdateSet()
                .InList();

            var (repoUpdater, packageUpdater) = MakeRepositoryUpdater(
                updateSelection, updates);

            var git = Substitute.For<IGitDriver>();
            var repo = MakeRepositoryData();

            var count = await repoUpdater.Run(git, repo, MakeSettings());

            Assert.That(count, Is.EqualTo(1));
            await AssertReceivedMakeUpdate(packageUpdater, 1);
        }

        [TestCase(0, true, 0, 0)]
        [TestCase(1, true, 1, 1)]
        [TestCase(2, true, 2, 1)]
        [TestCase(1, false, 1, 1)]
        [TestCase(2, false, 2, 2)]
        public async Task WhenThereAreUpdates_CountIsAsExpected(int numberOfUpdates, bool consolidateUpdates, int expectedUpdates, int expectedPrs)
        {
            var updateSelection = Substitute.For<IPackageUpdateSelection>();
            var collaborationFactory = Substitute.For<ICollaborationFactory>();
            var gitDriver = Substitute.For<IGitDriver>();
            UpdateSelectionAll(updateSelection);

            var packageUpdater = new PackageUpdater(collaborationFactory,
                Substitute.For<IUpdateRunner>(),
                Substitute.For<INuKeeperLogger>());

            var updates = Enumerable.Range(1, numberOfUpdates)
                .Select(_ => PackageUpdates.UpdateSet())
                .ToList();

            var settings = MakeSettings(consolidateUpdates);

            var (repoUpdater, _) = MakeRepositoryUpdater(
                updateSelection, updates, packageUpdater);

            var repo = MakeRepositoryData();

            gitDriver.GetCurrentHead().Returns("def");

            var count = await repoUpdater.Run(gitDriver, repo, settings);

            Assert.That(count, Is.EqualTo(expectedUpdates));

            await collaborationFactory.CollaborationPlatform.Received(expectedPrs)
                .OpenPullRequest(
                    Arg.Any<ForkData>(),
                    Arg.Any<PullRequestRequest>(),
                    Arg.Any<IEnumerable<string>>());

            gitDriver.Received(numberOfUpdates)
                .Commit(Arg.Any<string>());
        }

        [Test]
        public async Task WhenUpdatesAreFilteredOut_CountIsZero()
        {
            var updateSelection = Substitute.For<IPackageUpdateSelection>();
            UpdateSelectionNone(updateSelection);

            var twoUpdates = new List<PackageUpdateSet>
            {
                PackageUpdates.UpdateSet(),
                PackageUpdates.UpdateSet()
            };

            var (repoUpdater, packageUpdater) = MakeRepositoryUpdater(
                updateSelection,
                twoUpdates);

            var git = Substitute.For<IGitDriver>();
            var repo = MakeRepositoryData();

            var count = await repoUpdater.Run(git, repo, MakeSettings());

            Assert.That(count, Is.EqualTo(0));
            await AssertDidNotReceiveMakeUpdate(packageUpdater);
        }

        private static async Task AssertReceivedMakeUpdate(
            IPackageUpdater packageUpdater,
            int count)
        {
            await packageUpdater.Received(count)
                .MakeUpdatePullRequests(
                    Arg.Any<IGitDriver>(),
                Arg.Any<RepositoryData>(),
                Arg.Any<IReadOnlyCollection<PackageUpdateSet>>(),
                Arg.Any<NuGetSources>(),
                Arg.Any<SettingsContainer>());
        }

        private static async Task AssertDidNotReceiveMakeUpdate(
            IPackageUpdater packageUpdater)
        {
            await packageUpdater.DidNotReceiveWithAnyArgs()
                .MakeUpdatePullRequests(
                    Arg.Any<IGitDriver>(),
                Arg.Any<RepositoryData>(),
                Arg.Any<IReadOnlyCollection<PackageUpdateSet>>(),
                Arg.Any<NuGetSources>(),
                Arg.Any<SettingsContainer>());
        }

        private static void UpdateSelectionAll(IPackageUpdateSelection updateSelection)
        {
            updateSelection.SelectTargets(
                    Arg.Any<ForkData>(),
                    Arg.Any<IReadOnlyCollection<PackageUpdateSet>>(),
                    Arg.Any<FilterSettings>())
                .Returns(c => c.ArgAt<IReadOnlyCollection<PackageUpdateSet>>(1));
        }

        private static void UpdateSelectionNone(IPackageUpdateSelection updateSelection)
        {
            updateSelection.SelectTargets(
                    Arg.Any<ForkData>(),
                    Arg.Any<IReadOnlyCollection<PackageUpdateSet>>(),
                    Arg.Any<FilterSettings>())
                .Returns(new List<PackageUpdateSet>());
        }

        private SettingsContainer MakeSettings(bool consolidateUpdates = false)
        {
            return new SettingsContainer
            {
                SourceControlServerSettings = new SourceControlServerSettings(),
                UserSettings = new UserSettings
                {
                    ConsolidateUpdatesInSinglePullRequest = consolidateUpdates
                }
            };
        }

        private static
            (IRepositoryUpdater repositoryUpdater, IPackageUpdater packageUpdater) MakeRepositoryUpdater(
            IPackageUpdateSelection updateSelection,
            List<PackageUpdateSet> updates,
            IPackageUpdater packageUpdater = null)
        {
            var sources = Substitute.For<INuGetSourcesReader>();
            var updateFinder = Substitute.For<IUpdateFinder>();
            var fileRestore = Substitute.For<IFileRestoreCommand>();
            var reporter = Substitute.For<IReporter>();

            updateFinder.FindPackageUpdateSets(
                    Arg.Any<IFolder>(),
                    Arg.Any<NuGetSources>(),
                    Arg.Any<VersionChange>(),
                    Arg.Any<UsePrerelease>())
                .Returns(updates);

            if (packageUpdater == null)
            {
                packageUpdater = Substitute.For<IPackageUpdater>();
                packageUpdater.MakeUpdatePullRequests(
                        Arg.Any<IGitDriver>(),
                        Arg.Any<RepositoryData>(),
                        Arg.Any<IReadOnlyCollection<PackageUpdateSet>>(),
                        Arg.Any<NuGetSources>(),
                        Arg.Any<SettingsContainer>())
                    .Returns(1);
            }

            var repoUpdater = new RepositoryUpdater(
                sources, updateFinder, updateSelection, packageUpdater,
                Substitute.For<INuKeeperLogger>(), new SolutionsRestore(fileRestore),
                reporter);

            return (repoUpdater, packageUpdater);
        }

        private static RepositoryData MakeRepositoryData()
        {
            return new RepositoryData(
                new ForkData(new Uri("http://foo.com"), "me", "test"),
                new ForkData(new Uri("http://foo.com"), "me", "test"));
        }
    }
}
