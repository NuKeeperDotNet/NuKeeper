using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Engine;
using NuKeeper.Engine.Packages;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.Sort;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Update.Selection;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class PackageUpdateSelectionTests
    {
        [Test]
        public async Task WhenThereAreNoInputs_NoTargetsOut()
        {
            var updateSets = new List<PackageUpdateSet>();

            var target = SelectionForFilter(BranchFilter(true));

            var results = await target.SelectTargets(PushFork(), updateSets, NoFilter());

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task WhenThereIsOneInput_ItIsTheTarget()
        {
            var updateSets = UpdateFooFromOneVersion()
                .InList();

            var target = SelectionForFilter(BranchFilter(true));

            var results = await target.SelectTargets(PushFork(), updateSets, NoFilter());

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().SelectedId, Is.EqualTo("foo"));
        }

        [Test]
        public async Task WhenThereAreTwoInputs_MoreVersionsFirst_FirstIsTheTarget()
        {
            // sort should not change this ordering
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateBarFromTwoVersions(),
                UpdateFooFromOneVersion()
            };

            var target = SelectionForFilter(BranchFilter(true));

            var results = await target.SelectTargets(PushFork(), updateSets, NoFilter());

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.First().SelectedId, Is.EqualTo("bar"));
        }

        [Test]
        public async Task WhenThereAreTwoInputs_MoreVersionsSecond_SecondIsTheTarget()
        {
            // sort should change this ordering
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var target = SelectionForFilter(BranchFilter(true));

            var results = await target.SelectTargets(PushFork(), updateSets, NoFilter());

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.First().SelectedId, Is.EqualTo("bar"));
        }

        [Test]
        public async Task WhenExistingBranchesAreFilteredOut()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var filter = BranchFilter(false);

            var target = SelectionForFilter(filter);

            var results = await target.SelectTargets(PushFork(), updateSets, NoFilter());

            Assert.That(results.Count, Is.EqualTo(0));
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
                PackageUpdates.OfficialPackageSource(), pubDate, null);

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
            var match = new PackageSearchMedatadata(matchId, PackageUpdates.OfficialPackageSource(), pubDate, null);

            var updates = new PackageLookupResult(VersionChange.Major, match, null, null);
            return new PackageUpdateSet(updates, currentPackages);
        }

        private PackagePath PathToProjectOne()
        {
            return new PackagePath("c_temp", "projectOne", PackageReferenceType.PackagesConfig);
        }

        private PackagePath PathToProjectTwo()
        {
            return new PackagePath("c_temp", "projectTwo", PackageReferenceType.PackagesConfig);
        }

        private static IPackageUpdateSelection SelectionForFilter(IExistingBranchFilter filter)
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var updateSelection = new UpdateSelection(logger);
            return new PackageUpdateSelection(filter,
                MakeSort(), updateSelection, logger);
        }

        private FilterSettings NoFilter()
        {
            return new FilterSettings
            {
                MaxPackageUpdates = Int32.MaxValue,
                MinimumAge = TimeSpan.Zero
            };
        }

        private static ForkData PushFork()
        {
            return new ForkData(new Uri("http://github.com/foo/bar"), "me", "test");
        }

        private static IExistingBranchFilter BranchFilter(bool result)
        {
            var filter = Substitute.For<IExistingBranchFilter>();
            filter.CanMakeBranchFor(
                Arg.Any<PackageUpdateSet>(),
                    Arg.Any<ForkData>())
                .Returns(x => Task.FromResult(result));

            return filter;
        }

        private static IPackageUpdateSetSort MakeSort()
        {
            return new PackageUpdateSetSort(Substitute.For<INuKeeperLogger>());
        }
    }
}
