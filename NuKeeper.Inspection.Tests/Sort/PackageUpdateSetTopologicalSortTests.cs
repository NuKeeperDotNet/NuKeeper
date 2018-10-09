using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sort;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.Sort
{
    [TestFixture]
    public class PackageUpdateSetTopologicalSortTests
    {
        [Test]
        public void CanSortEmptyList()
        {
            var items = new List<PackageUpdateSet>();

            var sorter = new PackageUpdateSetTopologicalSort(Substitute.For<INuKeeperLogger>());

            var sorted = sorter.Sort(items)
                .ToList();

            Assert.That(sorted, Is.Not.Null);
            Assert.That(sorted, Is.Empty);
        }

        [Test]
        public void CanSortOneItemInList()
        {
            var items = new List<PackageUpdateSet>
            {
                MakeUpdateSet("foo", "1.2.3")
            };

            var sorter = new PackageUpdateSetTopologicalSort(Substitute.For<INuKeeperLogger>());

            var sorted = sorter.Sort(items)
                .ToList();

            AssertIsASortOf(sorted, items);
            Assert.That(sorted[0], Is.EqualTo(items[0]));
        }

        [Test]
        public void CanSortTwoUnrelatedItems()
        {
            var items = new List<PackageUpdateSet>
            {
                MakeUpdateSet("fish", "1.2.3"),
                MakeUpdateSet("bar", "2.3.4")
            };

            var logger = Substitute.For<INuKeeperLogger>();

            var sorter = new PackageUpdateSetTopologicalSort(logger);

            var sorted = sorter.Sort(items)
                .ToList();

            AssertIsASortOf(sorted, items);
            Assert.That(sorted[0], Is.EqualTo(items[0]));
            Assert.That(sorted[1], Is.EqualTo(items[1]));

            logger.Received(1).Detailed("No dependencies between items, no need to sort on dependencies");
            logger.Received(1).Detailed("Sorted 2 packages by dependencies but no change made");
        }

        [Test]
        public void CanSortTwoRelatedItemsInCorrectOrder()
        {
            var fishPackage = MakeUpdateSet("fish", "1.2.3");

            var items = new List<PackageUpdateSet>
            {
                fishPackage,
                MakeUpdateSet("bar", "2.3.4", DependencyOn(fishPackage))
            };


            var logger = Substitute.For<INuKeeperLogger>();

            var sorter = new PackageUpdateSetTopologicalSort(logger);

            var sorted = sorter.Sort(items)
                .ToList();

            AssertIsASortOf(sorted, items);
            Assert.That(sorted[0], Is.EqualTo(items[0]));
            Assert.That(sorted[1], Is.EqualTo(items[1]));

            logger.DidNotReceive().Detailed("No dependencies between items, no need to sort on dependencies");
            logger.Received(1).Detailed("Sorted 2 packages by dependencies but no change made");
        }

        [Test]
        public void CanSortTwoRelatedItemsinReverseOrder()
        {
            var fishPackage = MakeUpdateSet("fish", "1.2.3");

            var items = new List<PackageUpdateSet>
            {
                MakeUpdateSet("bar", "2.3.4", DependencyOn(fishPackage)),
                fishPackage
            };


            var logger = Substitute.For<INuKeeperLogger>();

            var sorter = new PackageUpdateSetTopologicalSort(logger);

            var sorted = sorter.Sort(items)
                .ToList();

            AssertIsASortOf(sorted, items);

            Assert.That(sorted[0], Is.EqualTo(items[1]));
            Assert.That(sorted[1], Is.EqualTo(items[0]));

            logger.DidNotReceive().Detailed("No dependencies between items, no need to sort on dependencies");
            logger.Received(1).Detailed("Resorted 2 packages by dependencies, first change is fish moved to position 0 from 1.");
        }

        [Test]
        public void CanSortThreeRelatePackages()
        {
            var apexPackage = MakeUpdateSet("apex", "1.2.3");

            var items = new List<PackageUpdateSet>
            {
                MakeUpdateSet("foo", "1.2.3", DependencyOn(apexPackage)),
                apexPackage,
                MakeUpdateSet("bar", "2.3.4", DependencyOn(apexPackage)),
            };


            var logger = Substitute.For<INuKeeperLogger>();

            var sorter = new PackageUpdateSetTopologicalSort(logger);

            var sorted = sorter.Sort(items)
                .ToList();


            AssertIsASortOf(sorted, items);
            Assert.That(sorted[0], Is.EqualTo(apexPackage));
        }

        [Test]
        public void CanSortWithCycle()
        {
            var pakageOne = MakeUpdateSet("one", "1.2.3");
            var packageAlpha = MakeUpdateSet("alpha", "2.3.4", DependencyOn(pakageOne));
            pakageOne = MakeUpdateSet("one", "1.2.3", DependencyOn(packageAlpha));

            var items = new List<PackageUpdateSet>
            {
                pakageOne,
                packageAlpha
            };

            var logger = Substitute.For<INuKeeperLogger>();

            var sorter = new PackageUpdateSetTopologicalSort(logger);

            var sorted = sorter.Sort(items)
                .ToList();

            AssertIsASortOf(sorted, items);
            logger.Received(1).Minimal(Arg.Is<string>(s => s.StartsWith("Cannot sort by dependencies, cycle found at item")));
        }


        private static void AssertIsASortOf(List<PackageUpdateSet> sorted, List<PackageUpdateSet> original)
        {
            Assert.That(sorted, Is.Not.Null);
            Assert.That(sorted, Is.Not.Empty);
            Assert.That(sorted.Count, Is.EqualTo(original.Count));
            CollectionAssert.AreEquivalent(sorted, original);
        }

        private PackageDependency DependencyOn(PackageUpdateSet package)
        {
            return new PackageDependency(package.SelectedId, new VersionRange(package.SelectedVersion));
        }

        private PackageUpdateSet MakeUpdateSet(string packageId, string packageVersion, PackageDependency upstream = null)
        {
            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject(packageId, packageVersion, PathToProjectOne()),
                new PackageInProject(packageId, packageVersion, PathToProjectTwo())
            };

            var majorUpdate = Metadata(packageId, packageVersion, upstream);

            var lookupResult = new PackageLookupResult(VersionChange.Major,
                majorUpdate, null, null);
            var updates = new PackageUpdateSet(lookupResult, currentPackages);

            return updates;
        }

        private PackagePath PathToProjectOne()
        {
            return new PackagePath("c_temp", "projectOne", PackageReferenceType.PackagesConfig);
        }

        private PackagePath PathToProjectTwo()
        {
            return new PackagePath("c_temp", "projectTwo", PackageReferenceType.PackagesConfig);
        }

        private static PackageSearchMedatadata Metadata(string packageId, string version, PackageDependency upstream)
        {
            var upstreams = new List<PackageDependency>();
            if (upstream != null)
            {
                upstreams.Add(upstream);
            }

            return new PackageSearchMedatadata(
                new PackageIdentity(packageId, new NuGetVersion(version)),
                new PackageSource(NuGetConstants.V3FeedUrl),
                DateTimeOffset.Now, upstreams);
        }

    }
}
