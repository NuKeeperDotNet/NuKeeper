using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Update.Selection;
using NUnit.Framework;

namespace NuKeeper.Update.Tests
{
    [TestFixture]
    public class UpdateSelectionTests
    {
        [Test]
        public async Task WhenThereAreNoInputs_NoTargetsOut()
        {
            var updateSets = new List<PackageUpdateSet>();

            var target = OneTargetSelection();

            var results = await target.Filter(updateSets, Pass);

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task WhenThereIsOneInput_ItIsTheTarget()
        {
            var updateSets = new List<PackageUpdateSet> { UpdateFooFromOneVersion() };

            var target = OneTargetSelection();

            var results = await target.Filter(updateSets, Pass);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().SelectedId, Is.EqualTo("foo"));
        }
        [Test]
        public async Task WhenThereAreTwoInputs_FirstIsTheTarget()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var target = OneTargetSelection();

            var results = await target.Filter(updateSets, Pass);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().SelectedId, Is.EqualTo("foo"));
        }

        [Test]
        public async Task WhenThereAreIncludes_OnlyConsiderMatches()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var settings = new FilterSettings
            {
                MaxPullRequests = 10,
                Includes = new Regex("bar")
            };

            var target = new UpdateSelection(settings, Substitute.For<INuKeeperLogger>());

            var results = await target.Filter(updateSets, Pass);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().SelectedId, Is.EqualTo("bar"));
        }

        [Test]
        public async Task WhenThereAreExcludes_OnlyConsiderNonMatching()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var settings = new FilterSettings
            {
                MaxPullRequests = 10,
                Excludes = new Regex("bar")
            };

            var target = new UpdateSelection(settings, Substitute.For<INuKeeperLogger>());

            var results = await target.Filter(updateSets, Pass);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().SelectedId, Is.EqualTo("foo"));
        }

        [Test]
        public async Task WhenThereAreIncludesAndExcludes_OnlyConsiderMatchesButRemoveNonMatching()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFoobarFromOneVersion(),
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var settings = new FilterSettings 
            {
                MaxPullRequests = 10,
                Excludes = new Regex("bar"),
                Includes = new Regex("foo")
            };

            var target = new UpdateSelection(settings, Substitute.For<INuKeeperLogger>());

            var results = await target.Filter(updateSets, Pass);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().SelectedId, Is.EqualTo("foo"));
        }

        [Test]
        public async Task WhenExistingBranchesAreFilteredOut()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var target = OneTargetSelection();

            var results = await target.Filter(updateSets, Fail);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task WhenFirstPackageIsFilteredOutByBranch()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var target = OneTargetSelection();

            var results = await target.Filter(updateSets, FilterToId("bar"));

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().SelectedId, Is.EqualTo("bar"));
        }

        [Test]
        public async Task WhenThePackageIsNotOldEnough()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion()
            };

            var target = MinAgeTargetSelection(TimeSpan.FromDays(7));

            var results = await target.Filter(updateSets, Pass);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task WhenTheFirstPackageIsNotOldEnough()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(TimeSpan.FromDays(6)),
                UpdateBarFromTwoVersions(TimeSpan.FromDays(8))
            };

            var target = MinAgeTargetSelection(TimeSpan.FromDays(7));

            var results = await target.Filter(updateSets, Pass);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().SelectedId, Is.EqualTo("bar"));
        }

        [Test]
        public async Task WhenMinAgeIsLowBothPackagesAreIncluded()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(TimeSpan.FromDays(6)),
                UpdateBarFromTwoVersions(TimeSpan.FromDays(8))
            };

            var target = MinAgeTargetSelection(TimeSpan.FromHours(12));

            var results = await target.Filter(updateSets, Pass);

            Assert.That(results.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task WhenMinAgeIsHighNeitherPackagesAreIncluded()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(TimeSpan.FromDays(6)),
                UpdateBarFromTwoVersions(TimeSpan.FromDays(8))
            };

            var target = MinAgeTargetSelection(TimeSpan.FromDays(10));

            var results = await target.Filter(updateSets, Pass);

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

            var latest = new PackageSearchMedatadata(newPackage, "ASource", DateTimeOffset.Now, null);

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
            var match = new PackageSearchMedatadata(new PackageIdentity("foo", matchVersion),
                "ASource", pubDate, null);

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
            var match = new PackageSearchMedatadata(matchId, "ASource", pubDate, null);

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

        private static IUpdateSelection OneTargetSelection()
        {
            const int maxPullRequests = 1;

            var settings = new FilterSettings
            {
                MaxPullRequests = maxPullRequests,
                MinimumAge = TimeSpan.Zero
            };
            return new UpdateSelection(settings, Substitute.For<INuKeeperLogger>());
        }

        private static IUpdateSelection MinAgeTargetSelection(TimeSpan minAge)
        {
            const int maxPullRequests = 1000;

            var settings = new FilterSettings
            {
                MaxPullRequests = maxPullRequests,
                MinimumAge = minAge
            };
            return new UpdateSelection(settings, Substitute.For<INuKeeperLogger>());
        }

        private static Task<bool> Pass(PackageUpdateSet s) => Task.FromResult(true);
        private static Task<bool> Fail(PackageUpdateSet s) => Task.FromResult(false);

        private Func<PackageUpdateSet, Task<bool>> FilterToId(string id)
        {
            return p => Task.FromResult(p.SelectedId == id);
        }
    }
}
