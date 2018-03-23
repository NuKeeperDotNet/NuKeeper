using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NSubstitute;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Configuration;
using NuKeeper.Engine.Packages;
using NuKeeper.Git;
using NuKeeper.NuGet.Api;
using NuKeeper.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class PackageUpdateSelectionTests
    {
        [Test]
        public void WhenThereAreNoInputs_NoTargetsOut()
        {
            var updateSets = Enumerable.Empty<PackageUpdateSet>();

            var target = OneTargetSelection();

            var results = target.SelectTargets(GitDriver(), updateSets);

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void WhenThereIsOneInput_ItIsTheTarget()
        {
            var updateSets = new List<PackageUpdateSet> { UpdateFooFromOneVersion() };

            var target = OneTargetSelection();

            var results = target.SelectTargets(GitDriver(), updateSets);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].SelectedId, Is.EqualTo("foo"));
        }

        [Test]
        public void WhenThereAreTwoInputs_MoreVersionsFirst_FirstIsTheTarget()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateBarFromTwoVersions(),
                UpdateFooFromOneVersion()
            };

            var target = OneTargetSelection();

            var results = target.SelectTargets(GitDriver(), updateSets);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].SelectedId, Is.EqualTo("bar"));
        }

        [Test]
        public void WhenThereAreTwoInputs_MoreVersionsSecond_SecondIsTheTarget()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var target = OneTargetSelection();

            var results = target.SelectTargets(GitDriver(), updateSets);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].SelectedId, Is.EqualTo("bar"));
        }

        [Test]
        public void WhenThereAreIncludes_OnlyConsiderMatches()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var settings = new UserSettings
            {
                MaxPullRequestsPerRepository = 10,
                PackageIncludes = new Regex("bar")
            };

            var target = new PackageUpdateSelection(settings,
                new NullNuKeeperLogger(), BranchFilter());

            var results = target.SelectTargets(GitDriver(), updateSets);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].SelectedId, Is.EqualTo("bar"));
        }

        [Test]
        public void WhenThereAreExcludes_OnlyConsiderNonMatching()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var settings = new UserSettings
            {
                MaxPullRequestsPerRepository = 10,
                PackageExcludes = new Regex("bar")
            };

            var target = new PackageUpdateSelection(settings,
                new NullNuKeeperLogger(), BranchFilter());

            var results = target.SelectTargets(GitDriver(), updateSets);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].SelectedId, Is.EqualTo("foo"));
        }

        [Test]
        public void WhenThereAreIncludesAndExcludes_OnlyConsiderMatchesButRemoveNonMatching()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFoobarFromOneVersion(),
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var settings = new UserSettings 
            {
                MaxPullRequestsPerRepository = 10,
                PackageExcludes = new Regex("bar"),
                PackageIncludes = new Regex("foo")
            };

            var target = new PackageUpdateSelection(settings,
                new NullNuKeeperLogger(), BranchFilter());

            var results = target.SelectTargets(GitDriver(), updateSets);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].SelectedId, Is.EqualTo("foo"));
        }

        [Test]
        public void WhenExistingBranchesAreFilteredOut()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var filter = BranchFilter(true);

            var target = OneTargetSelection(filter);

            var results = target.SelectTargets(GitDriver(), updateSets);

            Assert.That(results.Count, Is.EqualTo(0));
            filter.Received(2).HasExistingBranch(Arg.Any<IGitDriver>(), Arg.Any<PackageUpdateSet>());
        }

        [Test]
        public void WhenFirstPackageIsFilteredOutByBranch()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var filter = Substitute.For<IExistingBranchFilter>();

            filter.HasExistingBranch(
                Arg.Any<IGitDriver>(),
                Arg.Is<PackageUpdateSet>(s => s.SelectedId.Contains("foo"))).Returns(true);

            filter.HasExistingBranch(
                Arg.Any<IGitDriver>(),
                Arg.Is<PackageUpdateSet>(s => s.SelectedId.Contains("bar"))).Returns(false);

            var target = OneTargetSelection(filter);

            var results = target.SelectTargets(GitDriver(), updateSets);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].SelectedId, Is.EqualTo("bar"));
            filter.Received().HasExistingBranch(Arg.Any<IGitDriver>(), Arg.Any<PackageUpdateSet>());
        }

        [Test]
        public void WhenThePackageIsNotOldEnough()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion()
            };

            var target = MinAgeTargetSelection(TimeSpan.FromDays(7));

            var results = target.SelectTargets(GitDriver(), updateSets);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void WhenTheFirstPackageIsNotOldEnough()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(TimeSpan.FromDays(6)),
                UpdateBarFromTwoVersions(TimeSpan.FromDays(8))
            };

            var target = MinAgeTargetSelection(TimeSpan.FromDays(7));

            var results = target.SelectTargets(GitDriver(), updateSets);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].SelectedId, Is.EqualTo("bar"));
        }

        [Test]
        public void WhenMinAgeIsLowBothPackagesAreIncluded()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(TimeSpan.FromDays(6)),
                UpdateBarFromTwoVersions(TimeSpan.FromDays(8))
            };

            var target = MinAgeTargetSelection(TimeSpan.FromHours(12));

            var results = target.SelectTargets(GitDriver(), updateSets);

            Assert.That(results.Count, Is.EqualTo(2));
        }

        [Test]
        public void WhenMinAgeIsHighNeitherPackagesAreIncluded()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(TimeSpan.FromDays(6)),
                UpdateBarFromTwoVersions(TimeSpan.FromDays(8))
            };

            var target = MinAgeTargetSelection(TimeSpan.FromDays(10));

            var results = target.SelectTargets(GitDriver(), updateSets);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        private PackageUpdateSet UpdateFoobarFromOneVersion()
        {
            var newPackage = LatestVersionOfPackageFoobar();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foobar", "1.0.1", PathToProjectOne()),
                new PackageInProject("foobar", "1.0.1", PathToProjectTwo())
            };

            var latest = new PackageSearchMedatadata(newPackage, "ASource", DateTimeOffset.Now);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, currentPackages);
        }

        private PackageUpdateSet UpdateFooFromOneVersion(TimeSpan? packageAge = null)
        {
            var pubDate = DateTimeOffset.Now.Subtract(packageAge ?? TimeSpan.Zero);

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.1", PathToProjectOne()),
                new PackageInProject("foo", "1.0.1", PathToProjectTwo())
            };

            var matchVersion = new NuGetVersion("4.0.0");
            var match = new PackageSearchMedatadata(new PackageIdentity("foo", matchVersion), "ASource", pubDate);

            var updates = new PackageLookupResult(VersionChange.Major, match, null, null);
            return new PackageUpdateSet(updates, currentPackages);
        }

        private PackageUpdateSet UpdateBarFromTwoVersions(TimeSpan? packageAge = null)
        {
            var pubDate = DateTimeOffset.Now.Subtract(packageAge ?? TimeSpan.Zero);

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("bar", "1.0.1", PathToProjectOne()),
                new PackageInProject("bar", "1.2.1", PathToProjectTwo())
            };

            var matchId = new PackageIdentity("bar", new NuGetVersion("4.0.0"));
            var match = new PackageSearchMedatadata(matchId, "ASource", pubDate);

            var updates = new PackageLookupResult(VersionChange.Major, match, null, null);
            return new PackageUpdateSet(updates, currentPackages);
        }

        private PackageIdentity LatestVersionOfPackageFoobar()
        {
            return new PackageIdentity("foobar", new NuGetVersion("1.2.3"));
        }

        private PackagePath PathToProjectOne()
        {
            return new PackagePath("c_temp", "projectOne", PackageReferenceType.PackagesConfig);
        }

        private PackagePath PathToProjectTwo()
        {
            return new PackagePath("c_temp", "projectTwo", PackageReferenceType.PackagesConfig);
        }

        private static IPackageUpdateSelection OneTargetSelection()
        {
            return OneTargetSelection(BranchFilter());
        }

        private static IPackageUpdateSelection OneTargetSelection(IExistingBranchFilter filter)
        {
            const int maxPullRequests = 1;

            var settings = new UserSettings
            {
                MaxPullRequestsPerRepository = maxPullRequests,
                MinimumPackageAge = TimeSpan.Zero
            };
            return new PackageUpdateSelection(settings,
                new NullNuKeeperLogger(), filter);
        }

        private static IPackageUpdateSelection MinAgeTargetSelection(TimeSpan minAge)
        {
            const int maxPullRequests = 1000;

            var settings = new UserSettings
            {
                MaxPullRequestsPerRepository = maxPullRequests,
                MinimumPackageAge = minAge
            };
            return new PackageUpdateSelection(settings,
                new NullNuKeeperLogger(), BranchFilter());
        }

        private static IGitDriver GitDriver()
        {
            var git = Substitute.For<IGitDriver>();
            git.BranchExists(Arg.Any<string>()).Returns(false);
            return git;
        }

        private static IExistingBranchFilter BranchFilter(bool hasBranches = false)
        {
            var filter = Substitute.For<IExistingBranchFilter>();
            filter.HasExistingBranch(Arg.Any<IGitDriver>(), Arg.Any<PackageUpdateSet>())
                .Returns(hasBranches);
            return filter;
        }

    }
}
