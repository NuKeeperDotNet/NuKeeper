using NSubstitute;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Engine.Packages;
using NuKeeper.Inspection.Sort;
using NuKeeper.Update.Selection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class PackageUpdateSelectionTests
    {
        [Test]
        public void WhenThereAreNoInputs_NoTargetsOut()
        {
            var target = MakeSelection();

            var results = target.SelectTargets(PushFork(),
                new List<PackageUpdateSet>(), NoFilter());

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void WhenThereIsOneInput_ItIsTheTarget()
        {
            var updateSets = PackageUpdates.UpdateFooFromOneVersion()
                .InList();

            var target = MakeSelection();

            var results = target.SelectTargets(PushFork(), updateSets, NoFilter());

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results.First().SelectedId, Is.EqualTo("foo"));
        }

        [Test]
        public void WhenThereAreTwoInputs_MoreVersionsFirst_FirstIsTheTarget()
        {
            // sort should not change this ordering
            var updateSets = new List<PackageUpdateSet>
            {
                PackageUpdates.UpdateBarFromTwoVersions(),
                PackageUpdates.UpdateFooFromOneVersion()
            };

            var target = MakeSelection();

            var results = target.SelectTargets(PushFork(), updateSets, NoFilter());

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.First().SelectedId, Is.EqualTo("bar"));
        }

        [Test]
        public void WhenThereAreTwoInputs_MoreVersionsSecond_SecondIsTheTarget()
        {
            // sort should change this ordering
            var updateSets = new List<PackageUpdateSet>
            {
                PackageUpdates.UpdateFooFromOneVersion(),
                PackageUpdates.UpdateBarFromTwoVersions()
            };

            var target = MakeSelection();

            var results = target.SelectTargets(PushFork(), updateSets, NoFilter());

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results.First().SelectedId, Is.EqualTo("bar"));
        }

        private static IPackageUpdateSelection MakeSelection()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var updateSelection = new UpdateSelection(logger);
            return new PackageUpdateSelection(MakeSort(), updateSelection, logger);
        }

        private FilterSettings NoFilter()
        {
            return new FilterSettings
            {
                MaxPackageUpdates = Int32.MaxValue,
                MinimumAge = TimeSpan.Zero
            };
        }

        private BranchSettings DefaultBranchSettings()
        {
            return new BranchSettings();
        }

        private static ForkData PushFork()
        {
            return new ForkData(new Uri("http://github.com/foo/bar"), "me", "test");
        }

        private static IPackageUpdateSetSort MakeSort()
        {
            return new PackageUpdateSetSort(Substitute.For<INuKeeperLogger>());
        }
    }
}
