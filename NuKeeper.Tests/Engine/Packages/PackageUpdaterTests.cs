using NSubstitute;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.NuGetApi;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Engine;
using NuKeeper.Engine.Packages;
using NuKeeper.Update;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.Tests.Engine.Packages
{
    [TestFixture]
    public class PackageUpdaterTests
    {
        private ICollaborationFactory _collaborationFactory;
        private IExistingCommitFilter _existingCommitFilter;
        private IUpdateRunner _updateRunner;

        [SetUp]
        public void Initialize()
        {
            _collaborationFactory = Substitute.For<ICollaborationFactory>();
            _existingCommitFilter = Substitute.For<IExistingCommitFilter>();
            _updateRunner = Substitute.For<IUpdateRunner>();

            _existingCommitFilter
                .Filter(
                    Arg.Any<IGitDriver>(),
                    Arg.Any<IReadOnlyCollection<PackageUpdateSet>>(),
                    Arg.Any<string>(),
                    Arg.Any<string>()
                )
                .Returns(ci => ((IReadOnlyCollection<PackageUpdateSet>)ci[1]));
        }

        [Test]
        public async Task MakeUpdatePullRequests_TwoUpdatesOneExistingPrAndMaxOpenPrIsTwo_CreatesOnlyOnePr()
        {
            _collaborationFactory
                .CollaborationPlatform
                .GetNumberOfOpenPullRequests(Arg.Any<string>(), Arg.Any<string>())
                .Returns(1);
            var packages = new List<PackageUpdateSet>
            {
                MakePackageUpdateSet("foo.bar", "1.0.0"),
                MakePackageUpdateSet("notfoo.bar", "2.0.0")
            };
            var repoData = MakeRepositoryData();
            var settings = MakeSettings();
            settings.UserSettings.MaxOpenPullRequests = 2;
            var sut = MakePackageUpdater();

            var (updatesDone, thresholdReached) = await sut.MakeUpdatePullRequests(
                Substitute.For<IGitDriver>(),
                repoData,
                packages,
                new NuGetSources(""),
                settings
            );

            Assert.That(updatesDone, Is.EqualTo(1));
            Assert.That(thresholdReached, Is.True);
        }

        [Test]
        public async Task MakeUpdatePullRequest_OpenPrsEqualsMaxOpenPrs_DoesNotCreateNewPr()
        {
            _collaborationFactory
                .CollaborationPlatform
                .GetNumberOfOpenPullRequests(Arg.Any<string>(), Arg.Any<string>())
                .Returns(2);
            var packages = new List<PackageUpdateSet>
            {
                MakePackageUpdateSet("foo.bar", "1.0.0")
            };
            var repoData = MakeRepositoryData();
            var settings = MakeSettings();
            settings.UserSettings.MaxOpenPullRequests = 2;
            var sut = MakePackageUpdater();

            var (updatesDone, thresHoldReached) = await sut.MakeUpdatePullRequests(
                Substitute.For<IGitDriver>(),
                repoData,
                packages,
                new NuGetSources(""),
                settings
            );

            Assert.That(updatesDone, Is.EqualTo(0));
            Assert.That(thresHoldReached, Is.True);
        }

        [Test]
        public async Task MakeUpdatePullRequest_UpdateDoesNotCreatePrDueToExistingCommits_DoesNotPreventNewUpdates()
        {
            var packageSetOne = MakePackageUpdateSet("foo.bar", "1.0.0");
            var packageSetTwo = MakePackageUpdateSet("notfoo.bar", "2.0.0");
            _collaborationFactory
                .CollaborationPlatform
                .GetNumberOfOpenPullRequests(Arg.Any<string>(), Arg.Any<string>())
                .Returns(1);
            _existingCommitFilter
                .Filter(
                    Arg.Any<IGitDriver>(),
                    Arg.Any<IReadOnlyCollection<PackageUpdateSet>>(),
                    Arg.Any<string>(),
                    Arg.Any<string>()
                )
                .Returns(new List<PackageUpdateSet>(), new List<PackageUpdateSet> { packageSetTwo });
            var packages = new List<PackageUpdateSet> { packageSetOne, packageSetTwo };
            var repoData = MakeRepositoryData();
            var settings = MakeSettings();
            settings.UserSettings.MaxOpenPullRequests = 2;
            var sut = MakePackageUpdater();

            var (updatesDone, _) = await sut.MakeUpdatePullRequests(
                Substitute.For<IGitDriver>(),
                repoData,
                packages,
                new NuGetSources(""),
                settings
            );

            Assert.That(updatesDone, Is.EqualTo(1));
        }

        [Test]
        public async Task MakeUpdatePullRequest_UpdateDoesNotCreatePrDueToExistingPr_DoesNotPreventNewUpdates()
        {
            var packageSetOne = MakePackageUpdateSet("foo.bar", "1.0.0");
            var packageSetTwo = MakePackageUpdateSet("notfoo.bar", "2.0.0");
            _collaborationFactory
                .CollaborationPlatform
                .GetNumberOfOpenPullRequests(Arg.Any<string>(), Arg.Any<string>())
                .Returns(1);
            _collaborationFactory
                .CollaborationPlatform
                .PullRequestExists(Arg.Any<ForkData>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(true, false);
            var packages = new List<PackageUpdateSet> { packageSetOne, packageSetTwo };
            var repoData = MakeRepositoryData();
            var settings = MakeSettings();
            settings.UserSettings.MaxOpenPullRequests = 2;
            var sut = MakePackageUpdater();

            var (updatesDone, _) = await sut.MakeUpdatePullRequests(
                Substitute.For<IGitDriver>(),
                repoData,
                packages,
                new NuGetSources(""),
                settings
            );

            Assert.That(updatesDone, Is.EqualTo(2));
        }

        [Test]
        public async Task MakeUpdatePullRequests_LessPrsThanMaxOpenPrs_ReturnsNotThresholdReached()
        {
            _collaborationFactory
                .CollaborationPlatform
                .GetNumberOfOpenPullRequests(Arg.Any<string>(), Arg.Any<string>())
                .Returns(1);
            var packages = new List<PackageUpdateSet>
            {
                MakePackageUpdateSet("foo.bar", "1.0.0"),
                MakePackageUpdateSet("notfoo.bar", "2.0.0")
            };
            var repoData = MakeRepositoryData();
            var settings = MakeSettings();
            settings.UserSettings.MaxOpenPullRequests = 10;
            var sut = MakePackageUpdater();

            var (updatesDone, thresholdReached) = await sut.MakeUpdatePullRequests(
                Substitute.For<IGitDriver>(),
                repoData,
                packages,
                new NuGetSources(""),
                settings
            );

            Assert.That(thresholdReached, Is.False);
        }

        private PackageUpdater MakePackageUpdater()
        {
            return new PackageUpdater(
                _collaborationFactory,
                _existingCommitFilter,
                _updateRunner,
                Substitute.For<INuKeeperLogger>()
            );
        }

        private static RepositoryData MakeRepositoryData()
        {
            return new RepositoryData(
                new ForkData(new Uri("http://foo.com"), "me", "test"),
                new ForkData(new Uri("http://foo.com"), "me", "test"));
        }

        private static PackageUpdateSet MakePackageUpdateSet(string packageName, string version)
        {
            return new PackageUpdateSet(
                new PackageLookupResult(
                    VersionChange.Major,
                    MakePackageSearchMetadata(packageName, version),
                    null,
                    null
                ),
                new List<PackageInProject>
                {
                    MakePackageInProject(packageName, version)
                }
            );
        }

        private static SettingsContainer MakeSettings(
            bool consolidateUpdates = false
        )
        {
            return new SettingsContainer
            {
                SourceControlServerSettings = new SourceControlServerSettings
                {
                    Repository = new RepositorySettings()
                },
                UserSettings = new UserSettings
                {
                    ConsolidateUpdatesInSinglePullRequest = consolidateUpdates
                },
                BranchSettings = new BranchSettings(),
                PackageFilters = new FilterSettings
                {
                    MaxPackageUpdates = 3,
                    MinimumAge = new TimeSpan(7, 0, 0, 0),
                }
            };
        }

        private static PackageInProject MakePackageInProject(string packageName, string version)
        {
            return new PackageInProject(
                new PackageVersionRange(
                    packageName,
                    VersionRange.Parse(version)
                ),
                new PackagePath(
                    "projectA",
                    "MyFolder",
                    PackageReferenceType.PackagesConfig
                )
            );
        }

        private static PackageSearchMetadata MakePackageSearchMetadata(string packageName, string version)
        {
            return new PackageSearchMetadata(
                new PackageIdentity(
                    packageName,
                    NuGetVersion.Parse(version)
                ),
                new PackageSource("https://api.nuget.com/v3/"),
                new DateTimeOffset(2019, 1, 12, 0, 0, 0, TimeSpan.Zero),
                null
            );
        }
    }
}
