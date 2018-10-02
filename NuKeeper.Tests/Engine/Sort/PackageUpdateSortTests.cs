using NuGet.Packaging.Core;
using NuGet.Versioning;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSubstitute;
using NuGet.Configuration;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.Sort;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Tests.Engine.Sort
{
    [TestFixture]
    public class PackageUpdateSortTests
    {
        private static readonly DateTimeOffset StandardPublishedDate = new DateTimeOffset(2018, 2, 19, 11, 12, 7, TimeSpan.Zero);

        [Test]
        public void CanSortWhenListIsEmpty()
        {
            var items = new List<PackageUpdateSet>();

            var output = Sort(items);

            Assert.That(output, Is.Not.Null);
        }

        [Test]
        public void CanSortOneItem()
        {
            var items = new List<PackageUpdateSet>
            {
                OnePackageUpdateSet(1)
            };

            var output = Sort(items);

            Assert.That(output, Is.Not.Null);
            Assert.That(output.Count, Is.EqualTo(1));
            Assert.That(output[0], Is.EqualTo(items[0]));
        }

        [Test]
        public void CanSortTwoItems()
        {
            var items = new List<PackageUpdateSet>
            {
                OnePackageUpdateSet(1),
                OnePackageUpdateSet(2)
            };

            var output = Sort(items);

            Assert.That(output, Is.Not.Null);
            Assert.That(output.Count, Is.EqualTo(2));
        }

        [Test]
        public void CanSortThreeItems()
        {
            var items = new List<PackageUpdateSet>
            {
                OnePackageUpdateSet(1),
                OnePackageUpdateSet(2),
                OnePackageUpdateSet(3),
            };

            var output = Sort(items);

            Assert.That(output, Is.Not.Null);
            Assert.That(output.Count, Is.EqualTo(3));
        }

        [Test]
        public void TwoPackageVersionsIsSortedToTop()
        {
            var twoVersions = MakeTwoProjectVersions();
            var items = new List<PackageUpdateSet>
            {
                OnePackageUpdateSet(3),
                OnePackageUpdateSet(4),
                twoVersions
            };

            var output = Sort(items);

            Assert.That(output, Is.Not.Null);
            Assert.That(output[0], Is.EqualTo(twoVersions));
        }

        [Test]
        public void WillSortByProjectCount()
        {
            var items = new List<PackageUpdateSet>
            {
                OnePackageUpdateSet(1),
                OnePackageUpdateSet(2),
                OnePackageUpdateSet(3),
            };

            var output = Sort(items);

            Assert.That(output.Count, Is.EqualTo(3));
            Assert.That(output[0].CurrentPackages.Count, Is.EqualTo(3));
            Assert.That(output[1].CurrentPackages.Count, Is.EqualTo(2));
            Assert.That(output[2].CurrentPackages.Count, Is.EqualTo(1));
        }

        [Test]
        public void WillSortByProjectVersionsOverProjectCount()
        {
            var twoVersions = MakeTwoProjectVersions();
            var items = new List<PackageUpdateSet>
            {
                OnePackageUpdateSet(10),
                OnePackageUpdateSet(20),
                twoVersions,
            };

            var output = Sort(items);

            Assert.That(output.Count, Is.EqualTo(3));
            Assert.That(output[0], Is.EqualTo(twoVersions));
            Assert.That(output[1].CurrentPackages.Count, Is.EqualTo(20));
            Assert.That(output[2].CurrentPackages.Count, Is.EqualTo(10));
        }

        [Test]
        public void WillSortByBiggestVersionChange()
        {
            var items = new List<PackageUpdateSet>
            {
                PackageChange("1.2.4", "1.2.3"),
                PackageChange("2.0.0", "1.2.3"),
                PackageChange("1.3.0", "1.2.3")
            };

            var output = Sort(items);

            Assert.That(output.Count, Is.EqualTo(3));
            Assert.That(SelectedVersion(output[0]), Is.EqualTo("2.0.0"));
            Assert.That(SelectedVersion(output[1]), Is.EqualTo("1.3.0"));
            Assert.That(SelectedVersion(output[2]), Is.EqualTo("1.2.4"));
        }

        [Test]
        public void WillSortByGettingOutOfBetaFirst()
        {
            var items = new List<PackageUpdateSet>
            {
                PackageChange("2.0.0", "1.2.3"),
                PackageChange("1.2.4", "1.2.3-beta1"),
                PackageChange("1.3.0-pre-2", "1.2.3-beta1")
            };

            var output = Sort(items);

            Assert.That(output.Count, Is.EqualTo(3));
            Assert.That(SelectedVersion(output[0]), Is.EqualTo("1.2.4"));
            Assert.That(SelectedVersion(output[1]), Is.EqualTo("2.0.0"));
            Assert.That(SelectedVersion(output[2]), Is.EqualTo("1.3.0-pre-2"));
        }


        [Test]
        public void WillSortByOldestFirstOverPatchVersionIncrement()
        {
            var items = new List<PackageUpdateSet>
            {
                PackageChange("1.2.6", "1.2.3", StandardPublishedDate),
                PackageChange("1.2.5", "1.2.3", StandardPublishedDate.AddYears(-1)),
                PackageChange("1.2.4", "1.2.3", StandardPublishedDate.AddYears(-2))
            };

            var output = Sort(items);

            Assert.That(output.Count, Is.EqualTo(3));
            Assert.That(SelectedVersion(output[0]), Is.EqualTo("1.2.4"));
            Assert.That(SelectedVersion(output[1]), Is.EqualTo("1.2.5"));
            Assert.That(SelectedVersion(output[2]), Is.EqualTo("1.2.6"));
        }

        private string SelectedVersion(PackageUpdateSet packageUpdateSet)
        {
            return packageUpdateSet.Selected.Identity.Version.ToString();
        }

        private static PackageUpdateSet UpdateSetFor(PackageIdentity package, params PackageInProject[] packages)
        {
            return UpdateSetFor(package, StandardPublishedDate, packages);
        }

        private static PackageUpdateSet UpdateSetFor(PackageIdentity package, DateTimeOffset published, params PackageInProject[] packages)
        {
            var latest = new PackageSearchMedatadata(package, new PackageSource("http://none"), published, null);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null, null);
            return new PackageUpdateSet(updates, packages);
        }

        private static PackageInProject MakePackageInProjectFor(PackageIdentity package)
        {
            var path = new PackagePath(
                Path.GetTempPath(),
                Path.Combine("folder", "src", "project1", "packages.config"),
                PackageReferenceType.PackagesConfig);
            return new PackageInProject(package.Id, package.Version.ToString(), path);
        }

        private static PackageUpdateSet OnePackageUpdateSet(int projectCount)
        {
            var newPackage = new PackageIdentity("foo.bar", new NuGetVersion("1.4.5-preview006"));
            var package = new PackageIdentity("foo.bar", new NuGetVersion("1.2.3-preview004"));

            var projects = new List<PackageInProject>();
            foreach(var i in Enumerable.Range(1, projectCount))
            {
                projects.Add(MakePackageInProjectFor(package));
            }

            return UpdateSetFor(newPackage, projects.ToArray());
        }

        private PackageUpdateSet MakeTwoProjectVersions()
        {
            var newPackage = new PackageIdentity("foo.bar", new NuGetVersion("1.4.5-preview006"));

            var package123 = new PackageIdentity("foo.bar", new NuGetVersion("1.2.3-preview004"));
            var package124 = new PackageIdentity("foo.bar", new NuGetVersion("1.2.4-preview005"));
            var projects = new List<PackageInProject>
            {
                MakePackageInProjectFor(package123),
                MakePackageInProjectFor(package124),
            };

            return UpdateSetFor(newPackage, projects.ToArray());
        }

        private static PackageUpdateSet PackageChange(string newVersion, string oldVersion, DateTimeOffset? publishedDate = null)
        {
            var newPackage = new PackageIdentity("foo.bar", new NuGetVersion(newVersion));
            var oldPackage = new PackageIdentity("foo.bar", new NuGetVersion(oldVersion));

            if (!publishedDate.HasValue)
            {
                publishedDate = StandardPublishedDate;
            }

            var projects = new List<PackageInProject>
            {
                MakePackageInProjectFor(oldPackage)
            };

            return UpdateSetFor(newPackage, publishedDate.Value, projects.ToArray());
        }

        private List<PackageUpdateSet> Sort(IReadOnlyCollection<PackageUpdateSet> input)
        {
            var sorter = new PackageUpdateSetSort(Substitute.For<INuKeeperLogger>());
            return sorter.Sort(input)
                .ToList();
        }
    }
}
