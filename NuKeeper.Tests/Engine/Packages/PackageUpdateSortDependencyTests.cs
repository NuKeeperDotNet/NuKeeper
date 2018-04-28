using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Engine.Packages;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine.Packages
{
    [TestFixture]
    public class PackageUpdateSortDependencyTests
    {
        private static readonly DateTimeOffset StandardPublishedDate = new DateTimeOffset(2018, 2, 19, 11, 12, 7, TimeSpan.Zero);

        [Test]
        public void WillSortByProjectCountWhenThereAreNoDeps()
        {
            var upstream = OnePackageUpdateSet("upstream", 1, null);
            var downstream = OnePackageUpdateSet("downstream", 2, null);

            var items = new List<PackageUpdateSet>
            {
                downstream,
                upstream
            };
            var output = PackageUpdateSort.Sort(items)
                .ToList();

            Assert.That(output.Count, Is.EqualTo(2));
            Assert.That(output[0].SelectedId, Is.EqualTo("downstream"));
            Assert.That(output[1].SelectedId, Is.EqualTo("upstream"));
        }

        [Test]
        public void WillSortByDependencyWhenItExists()
        {
            var upstream = OnePackageUpdateSet("upstream", 1, null);
            var depOnUpstream = new List<PackageDependency>
            {
                new PackageDependency("upstream", VersionRange.All)
            };

            var downstream = OnePackageUpdateSet("downstream", 2, depOnUpstream);

            var items = new List<PackageUpdateSet>
            {
                downstream,
                upstream
            };

            var output = PackageUpdateSort.Sort(items)
                .ToList();

            Assert.That(output.Count, Is.EqualTo(2));
            Assert.That(output[0].SelectedId, Is.EqualTo("upstream"));
            Assert.That(output[1].SelectedId, Is.EqualTo("downstream"));
        }

        [Test]
        public void WillSortSecondAndThirdByDependencyWhenItExists()
        {
            var upstream = OnePackageUpdateSet("upstream", 1, null);
            var depOnUpstream = new List<PackageDependency>
            {
                new PackageDependency("upstream", VersionRange.All)
            };

            var downstream = OnePackageUpdateSet("downstream", 2, depOnUpstream);

            var items = new List<PackageUpdateSet>
            {
                OnePackageUpdateSet("nodeps", 3, null),
                downstream,
                upstream
            };

            var output = PackageUpdateSort.Sort(items)
                .ToList();

            Assert.That(output.Count, Is.EqualTo(3));
            Assert.That(output[0].SelectedId, Is.EqualTo("nodeps"));
            Assert.That(output[1].SelectedId, Is.EqualTo("upstream"));
            Assert.That(output[2].SelectedId, Is.EqualTo("downstream"));
        }

        [Test]
        public void SortWithThreeLevels()
        {
            var level1 = OnePackageUpdateSet("l1", 1, null);
            var depOnLevel1 = new List<PackageDependency>
            {
                new PackageDependency("l1", VersionRange.All)
            };

            var level2 = OnePackageUpdateSet("l2", 2, depOnLevel1);
            var depOnLevel2 = new List<PackageDependency>
            {
                new PackageDependency("l2", VersionRange.All)
            };

            var level3 = OnePackageUpdateSet("l3", 2, depOnLevel2);

            var items = new List<PackageUpdateSet>
            {
                level3,
                level2,
                level1
            };

            var output = PackageUpdateSort.Sort(items)
                .ToList();

            Assert.That(output.Count, Is.EqualTo(3));
            Assert.That(output[0].SelectedId, Is.EqualTo("l1"));
            Assert.That(output[1].SelectedId, Is.EqualTo("l2"));
            Assert.That(output[2].SelectedId, Is.EqualTo("l3"));
        }

        private static PackageUpdateSet OnePackageUpdateSet(string packageName, int projectCount,
            List<PackageDependency> deps)
        {
            var newPackage = new PackageIdentity(packageName, new NuGetVersion("1.4.5"));
            var package = new PackageIdentity(packageName, new NuGetVersion("1.2.3"));

            var projects = new List<PackageInProject>();
            foreach (var i in Enumerable.Range(1, projectCount))
            {
                projects.Add(MakePackageInProjectFor(package));
            }

            return UpdateSetFor(newPackage, projects, deps);
        }

        private static PackageInProject MakePackageInProjectFor(PackageIdentity package)
        {
            var path = new PackagePath(
                Path.GetTempPath(),
                Path.Combine("folder", "src", "project1", "packages.config"),
                PackageReferenceType.PackagesConfig);
            return new PackageInProject(package.Id, package.Version.ToString(), path);
        }

        private static PackageUpdateSet UpdateSetFor(PackageIdentity package,
            List<PackageInProject> packages, List<PackageDependency> deps)
        {
            return UpdateSetFor(package, StandardPublishedDate, packages, deps);
        }

        private static PackageUpdateSet UpdateSetFor(PackageIdentity package, DateTimeOffset published,
            List<PackageInProject> packages, List<PackageDependency> deps)
        {
            var latest = new PackageSearchMedatadata(package, "someSource", published, deps);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, packages);
        }
    }
}
