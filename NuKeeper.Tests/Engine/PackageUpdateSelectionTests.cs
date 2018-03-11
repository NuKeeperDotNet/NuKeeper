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

            var results = target.SelectTargets(GitWithNoBranches(), updateSets);

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void WhenThereIsOneInput_ItIsTheTarget()
        {
            var updateSets = new List<PackageUpdateSet> { UpdateFooFromOneVersion() };

            var target = OneTargetSelection();

            var results = target.SelectTargets(GitWithNoBranches(), updateSets);

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

            var results = target.SelectTargets(GitWithNoBranches(), updateSets);

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

            var results = target.SelectTargets(GitWithNoBranches(), updateSets);

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

            var target = new PackageUpdateSelection(settings, new NullNuKeeperLogger());

            var results = target.SelectTargets(GitWithNoBranches(), updateSets);

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

            var target = new PackageUpdateSelection(settings, new NullNuKeeperLogger());

            var results = target.SelectTargets(GitWithNoBranches(), updateSets);

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

            var target = new PackageUpdateSelection(settings, new NullNuKeeperLogger());

            var results = target.SelectTargets(GitWithNoBranches(), updateSets);

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

            var git = GitWithAllBranches();

            var target = OneTargetSelection();

            var results = target.SelectTargets(git, updateSets);

            Assert.That(results.Count, Is.EqualTo(0));
            git.Received(2).BranchExists(Arg.Any<string>());
        }

        [Test]
        public void WhenFirstPackageIsFilteredOutByBranch()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var git = Substitute.For<IGitDriver>();
            git.BranchExists(Arg.Is<string>(s => s.Contains("foo"))).Returns(true);
            git.BranchExists(Arg.Is<string>(s => s.Contains("bar"))).Returns(false);

            var target = OneTargetSelection();

            var results = target.SelectTargets(git, updateSets);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].SelectedId, Is.EqualTo("bar"));
            git.Received().BranchExists(Arg.Any<string>());
        }

        [Test]
        public void WhenThePackageIsNotOldEnough()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion()
            };

            var target = OneTargetSelection(TimeSpan.FromDays(7));

            var results = target.SelectTargets(GitWithNoBranches(), updateSets);

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

            var target = OneTargetSelection(TimeSpan.FromDays(7));

            var results = target.SelectTargets(GitWithNoBranches(), updateSets);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].SelectedId, Is.EqualTo("bar"));
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

        private static IPackageUpdateSelection OneTargetSelection(TimeSpan? minAge = null)
        {
            const int maxPullRequests = 1;

            var settings = new UserSettings
            {
                MaxPullRequestsPerRepository = maxPullRequests,
                MinimumPackageAge = minAge ?? TimeSpan.Zero
            };
            return new PackageUpdateSelection(settings, new NullNuKeeperLogger());
        }

        private static IGitDriver GitWithAllBranches()
        {
            var git = Substitute.For<IGitDriver>();
            git.BranchExists(Arg.Any<string>()).Returns(true);
            return git;
        }

        private static IGitDriver GitWithNoBranches()
        {
            var git = Substitute.For<IGitDriver>();
            git.BranchExists(Arg.Any<string>()).Returns(false);
            return git;
        }
    }
}
