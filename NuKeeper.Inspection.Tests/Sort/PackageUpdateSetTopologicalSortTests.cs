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

            Assert.That(sorted, Is.Not.Null);
            Assert.That(sorted, Is.Not.Empty);
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

            var sorter = new PackageUpdateSetTopologicalSort(Substitute.For<INuKeeperLogger>());

            var sorted = sorter.Sort(items)
                .ToList();

            Assert.That(sorted, Is.Not.Null);
            Assert.That(sorted, Is.Not.Empty);
            Assert.That(sorted[0], Is.EqualTo(items[0]));
            Assert.That(sorted[1], Is.EqualTo(items[1]));
        }

        private PackageUpdateSet MakeUpdateSet(string packageId, string packageVersion)
        {
            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject(packageId, packageVersion, PathToProjectOne()),
                new PackageInProject(packageId, packageVersion, PathToProjectTwo())
            };

            var lookupResult = new PackageLookupResult(VersionChange.Major,
                Metadata(packageId, packageVersion), null, null);
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

        private PackageSearchMedatadata Metadata(string packageId, string version)
        {
            return new PackageSearchMedatadata(
                new PackageIdentity(packageId, new NuGetVersion(version)),
                new PackageSource(NuGetConstants.V3FeedUrl),
                DateTimeOffset.Now, null);
        }

    }
}
