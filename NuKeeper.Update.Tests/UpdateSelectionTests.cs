using NSubstitute;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGetApi;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Update.Selection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuKeeper.Update.Tests
{
    [TestFixture]
    public class UpdateSelectionTests
    {
        [Test]
        public void WhenThereAreNoInputs_NoTargetsOut()
        {
            var updateSets = new List<PackageUpdateSet>();

            var target = CreateUpdateSelection();

            var results = target.Filter(updateSets, OneTargetSelection());

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void WhenThereIsOneInput_ItIsTheTarget()
        {
            var updateSets = new List<PackageUpdateSet> { UpdateFooFromOneVersion() };

            var target = CreateUpdateSelection();

            var results = target.Filter(updateSets, OneTargetSelection());

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().SelectedId, Is.EqualTo("foo"));
        }

        [Test]
        public void WhenThereAreTwoInputs_FirstIsTheTarget()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var target = CreateUpdateSelection();

            var results = target.Filter(updateSets, OneTargetSelection());

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().SelectedId, Is.EqualTo("foo"));
        }

        [Test]
        public void WhenThePackageIsNotOldEnough()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion()
            };

            var target = CreateUpdateSelection();
            var settings = MinAgeTargetSelection(TimeSpan.FromDays(7));

            var results = target.Filter(updateSets, settings);

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

            var target = CreateUpdateSelection();
            var settings = MinAgeTargetSelection(TimeSpan.FromDays(7));

            var results = target.Filter(updateSets, settings);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().SelectedId, Is.EqualTo("bar"));
        }

        [Test]
        public void WhenMinAgeIsLowBothPackagesAreIncluded()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(TimeSpan.FromDays(6)),
                UpdateBarFromTwoVersions(TimeSpan.FromDays(8))
            };

            var target = CreateUpdateSelection();
            var settings = MinAgeTargetSelection(TimeSpan.FromHours(12));

            var results = target.Filter(updateSets, settings);

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

            var target = CreateUpdateSelection();
            var settings = MinAgeTargetSelection(TimeSpan.FromDays(10));

            var results = target.Filter(updateSets, settings);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void WhenThePackageIsFromTheFuture()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(TimeSpan.FromMinutes(-5))
            };

            var target = CreateUpdateSelection();
            var settings = MinAgeTargetSelection(TimeSpan.FromDays(7));

            var results = target.Filter(updateSets, settings);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test]
        public void WhenMinAgeIsZeroAndThePackageIsFromTheFuture()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(TimeSpan.FromMinutes(-5))
            };

            var target = CreateUpdateSelection();
            var settings = MinAgeTargetSelection(TimeSpan.FromDays(0));

            var results = target.Filter(updateSets, settings);

            Assert.That(results.Count, Is.EqualTo(1));
        }

        private static PackageUpdateSet UpdateFoobarFromOneVersion()
        {
            var newPackage = LatestVersionOfPackageFoobar();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foobar", "1.0.1", PathToProjectOne()),
                new PackageInProject("foobar", "1.0.1", PathToProjectTwo())
            };

            var latest = new PackageSearchMetadata(newPackage, new PackageSource("http://none"), DateTimeOffset.Now, null);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, currentPackages);
        }

        private static PackageUpdateSet UpdateFooFromOneVersion(TimeSpan? packageAge = null)
        {
            var pubDate = DateTimeOffset.Now.Subtract(packageAge ?? TimeSpan.Zero);

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.1", PathToProjectOne()),
                new PackageInProject("foo", "1.0.1", PathToProjectTwo())
            };

            var matchVersion = new NuGetVersion("4.0.0");
            var match = new PackageSearchMetadata(new PackageIdentity("foo", matchVersion),
                new PackageSource("http://none"), pubDate, null);

            var updates = new PackageLookupResult(VersionChange.Major, match, null, null);
            return new PackageUpdateSet(updates, currentPackages);
        }

        private static PackageUpdateSet UpdateBarFromTwoVersions(TimeSpan? packageAge = null)
        {
            var pubDate = DateTimeOffset.Now.Subtract(packageAge ?? TimeSpan.Zero);

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("bar", "1.0.1", PathToProjectOne()),
                new PackageInProject("bar", "1.2.1", PathToProjectTwo())
            };

            var matchId = new PackageIdentity("bar", new NuGetVersion("4.0.0"));
            var match = new PackageSearchMetadata(matchId, new PackageSource("http://none"), pubDate, null);

            var updates = new PackageLookupResult(VersionChange.Major, match, null, null);
            return new PackageUpdateSet(updates, currentPackages);
        }

        private static PackageIdentity LatestVersionOfPackageFoobar()
        {
            return new PackageIdentity("foobar", new NuGetVersion("1.2.3"));
        }

        private static PackagePath PathToProjectOne()
        {
            return new PackagePath("c_temp", "projectOne", PackageReferenceType.PackagesConfig);
        }

        private static PackagePath PathToProjectTwo()
        {
            return new PackagePath("c_temp", "projectTwo", PackageReferenceType.PackagesConfig);
        }

        private static IUpdateSelection CreateUpdateSelection()
        {
            return new UpdateSelection(Substitute.For<INuKeeperLogger>());
        }

        private static FilterSettings OneTargetSelection()
        {
            const int maxPullRequests = 1;

            return new FilterSettings
            {
                MaxPackageUpdates = maxPullRequests,
                MinimumAge = TimeSpan.Zero
            };
        }

        private static FilterSettings MinAgeTargetSelection(TimeSpan minAge)
        {
            const int maxPullRequests = 1000;

            return new FilterSettings
            {
                MaxPackageUpdates = maxPullRequests,
                MinimumAge = minAge
            };
        }
    }
}
