using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Engine.Packages;
using NuKeeper.NuGet.Api;
using NuKeeper.RepositoryInspection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuKeeper.Tests.Engine.Packages
{
    [TestFixture]
    public class PackageUpdateSortTests
    {
        [Test]
        public void CanSortWhenListIsEmpty()
        {
            var items = new List<PackageUpdateSet>();

            var output = PackageUpdateSort.Sort(items)
                .ToList();

            Assert.That(output, Is.Not.Null);
        }

        [Test]
        public void CanSortOneItem()
        {
            var items = new List<PackageUpdateSet>
            {
                OnePackageUpdateSet(1)
            };

            var output = PackageUpdateSort.Sort(items)
                .ToList();

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

            var output = PackageUpdateSort.Sort(items)
                .ToList();

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

            var output = PackageUpdateSort.Sort(items)
                .ToList();

            Assert.That(output, Is.Not.Null);
            Assert.That(output.Count, Is.EqualTo(3));
        }

        [Test]
        public void TwoPackageVersionsIsSortedToTop()
        {
            var package123 = new PackageIdentity("foo.bar", new NuGetVersion("1.2.3"));
            var package124 = new PackageIdentity("foo.bar", new NuGetVersion("1.2.4"));
            var projects = new List<PackageInProject>
            {
                MakePackageInProjectFor(package123),
                MakePackageInProjectFor(package124),
            };

            var twoVersions = UpdateSetFor(package124, projects.ToArray());

            var items = new List<PackageUpdateSet>
            {
                OnePackageUpdateSet(3),
                OnePackageUpdateSet(4),
                twoVersions
            };

            var output = PackageUpdateSort.Sort(items)
                .ToList();

            Assert.That(output, Is.Not.Null);
            Assert.That(output[0], Is.EqualTo(twoVersions));
        }

        private static PackageUpdateSet UpdateSetFor(PackageIdentity package, params PackageInProject[] packages)
        {
            var publishedDate = new DateTimeOffset(2018, 2, 19, 11, 12, 7, TimeSpan.Zero);
            var latest = new PackageSearchMedatadata(package, "someSource", publishedDate);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, packages);
        }

        private static PackageInProject MakePackageInProjectFor(PackageIdentity package)
        {
            var path = new PackagePath("c:\\temp", "folder\\src\\project1\\packages.config",
                PackageReferenceType.PackagesConfig);
            return new PackageInProject(package.Id, package.Version.ToString(), path);
        }

        private static PackageUpdateSet OnePackageUpdateSet(int projectCount)
        {
            var package = new PackageIdentity("foo.bar", new NuGetVersion("1.2.3"));

            var projects = new List<PackageInProject>();
            foreach(int i in Enumerable.Range(1, projectCount))
            {
                projects.Add(MakePackageInProjectFor(package));
            }

            return UpdateSetFor(package, projects.ToArray());
        }
    }
}
