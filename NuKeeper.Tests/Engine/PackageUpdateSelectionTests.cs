using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NSubstitute;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Configuration;
using NuKeeper.Engine;
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
            Assert.That(results[0].PackageId, Is.EqualTo("foo"));
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
            Assert.That(results[0].PackageId, Is.EqualTo("bar"));
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
            Assert.That(results[0].PackageId, Is.EqualTo("bar"));
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
            Assert.That(results[0].PackageId, Is.EqualTo("bar"));
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
            Assert.That(results[0].PackageId, Is.EqualTo("foo"));
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
            Assert.That(results[0].PackageId, Is.EqualTo("foo"));
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
            Assert.That(results[0].PackageId, Is.EqualTo("bar"));
            git.Received().BranchExists(Arg.Any<string>());
        }

        private PackageUpdateSet UpdateFoobarFromOneVersion()
        {
            var newPackage = LatestVersionOfPackageFoobar();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foobar", "1.0.1", PathToProjectOne()),
                new PackageInProject("foobar", "1.0.1", PathToProjectTwo())
            };

            return new PackageUpdateSet(newPackage, "ASource", 
                new NuGetVersion("4.0.0"), VersionChange.Major, 
                currentPackages);
        }

        private PackageUpdateSet UpdateFooFromOneVersion()
        {
            var newPackage = LatestVersionOfPackageFoo();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.1", PathToProjectOne()),
                new PackageInProject("foo", "1.0.1", PathToProjectTwo())
            };

            return new PackageUpdateSet(newPackage, "ASource",
                new NuGetVersion("4.0.0"), VersionChange.Major,
                currentPackages);
        }

        private PackageUpdateSet UpdateBarFromTwoVersions()
        {
            var newPackage = LatestVersionOfPackageBar();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("bar", "1.0.1", PathToProjectOne()),
                new PackageInProject("bar", "1.2.1", PathToProjectTwo())
            };

            return new PackageUpdateSet(newPackage, "ASource", 
                new NuGetVersion("4.0.0"), VersionChange.Major, 
                currentPackages);
        }

        private PackageIdentity LatestVersionOfPackageFoobar()
        {
            return new PackageIdentity("foobar", new NuGetVersion("1.2.3"));
        }

        private PackageIdentity LatestVersionOfPackageFoo()
        {
            return new PackageIdentity("foo", new NuGetVersion("1.2.3"));
        }

        private PackageIdentity LatestVersionOfPackageBar()
        {
            return new PackageIdentity("bar", new NuGetVersion("2.3.4"));
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
            const int maxPullRequests = 1;
            var settings = new UserSettings
            {
                MaxPullRequestsPerRepository = maxPullRequests
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
