using NSubstitute;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Engine;
using NuKeeper.Engine.Packages;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update;
using NuKeeper.Update.Process;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

#pragma warning disable CA1801
        [TestCase(0, 0, true, true, 0, 0)]
        [TestCase(1, 0, true, true, 1, 0)]
        [TestCase(2, 0, true, true, 2, 0)]
        [TestCase(3, 0, true, true, 3, 0)]
        [TestCase(1, 1, true, true, 0, 0)]
        [TestCase(2, 1, true, true, 1, 0)]
        [TestCase(3, 1, true, true, 2, 0)]
        [TestCase(1, 0, false, true, 1, 0)]
        [TestCase(1, 1, false, true, 0, 0)]
        [TestCase(1, 0, true, false, 1, 1)]
        [TestCase(2, 0, true, false, 2, 1)]
        [TestCase(3, 0, true, false, 3, 1)]
        [TestCase(1, 1, true, false, 0, 0)]
        [TestCase(2, 1, true, false, 1, 1)]
        [TestCase(3, 1, true, false, 2, 1)]
        [TestCase(1, 0, false, false, 1, 1)]
        [TestCase(1, 1, false, false, 0, 0)]

        public async Task WhenThereAreUpdates_CountIsAsExpected(int numberOfUpdates, int existingCommitsPerBranch, bool consolidateUpdates, bool pullRequestExists, int expectedUpdates, int expectedPrs)
        {
            var updateSelection = Substitute.For<IPackageUpdateSelection>();
            var collaborationFactory = Substitute.For<ICollaborationFactory>();
            var gitDriver = Substitute.For<IGitDriver>();
            var existingCommitFilder = Substitute.For<IExistingCommitFilter>();
            UpdateSelectionAll(updateSelection);

            gitDriver.GetCurrentHead().Returns("def");
            gitDriver.CheckoutNewBranch(Arg.Any<string>()).Returns(true);

            collaborationFactory
                .CollaborationPlatform
                .PullRequestExists(Arg.Any<ForkData>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(pullRequestExists);

            var packageUpdater = new PackageUpdater(collaborationFactory,
                existingCommitFilder,
                Substitute.For<IUpdateRunner>(),
                Substitute.For<INuKeeperLogger>());

            var updates = Enumerable.Range(1, numberOfUpdates)
                .Select(_ => PackageUpdates.UpdateSet())
                .ToList();

            var filteredUpdates = updates.Skip(existingCommitsPerBranch).ToList().AsReadOnly();

            existingCommitFilder.Filter(Arg.Any<IGitDriver>(), Arg.Any<IReadOnlyCollection<PackageUpdateSet>>(), Arg.Any<string>(), Arg.Any<string>()).Returns(filteredUpdates);

            var settings = MakeSettings(consolidateUpdates);

            var (repoUpdater, _) = MakeRepositoryUpdater(
                updateSelection, updates, packageUpdater);

            var repo = MakeRepositoryData();

            var count = await repoUpdater.Run(gitDriver, repo, settings);

            Assert.That(count, Is.EqualTo(expectedUpdates), "Returned count of updates");

            await collaborationFactory.CollaborationPlatform.Received(expectedPrs)
                .OpenPullRequest(
                    Arg.Any<ForkData>(),
                    Arg.Any<PullRequestRequest>(),
                    Arg.Any<IEnumerable<string>>());

            await gitDriver.Received(expectedUpdates).Commit(Arg.Any<string>());
        }
#pragma warning restore CA1801

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
                },
                BranchSettings = new BranchSettings()
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
