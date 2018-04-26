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
        public void WillSortByProjectCount()
        {
            var items = new List<PackageUpdateSet>
            {
                OnePackageUpdateSet(1),
                OnePackageUpdateSet(2)
            };

            var output = PackageUpdateSort.Sort(items)
                .ToList();

            Assert.That(output.Count, Is.EqualTo(2));
            Assert.That(output[0].CurrentPackages.Count, Is.EqualTo(2));
            Assert.That(output[1].CurrentPackages.Count, Is.EqualTo(1));
        }

        private static PackageUpdateSet OnePackageUpdateSet(int projectCount)
        {
            var newPackage = new PackageIdentity("foo.bar", new NuGetVersion("1.4.5"));
            var package = new PackageIdentity("foo.bar", new NuGetVersion("1.2.3"));

            var projects = new List<PackageInProject>();
            foreach (var i in Enumerable.Range(1, projectCount))
            {
                projects.Add(MakePackageInProjectFor(package));
            }

            return UpdateSetFor(newPackage, projects.ToArray());
        }

        private static PackageInProject MakePackageInProjectFor(PackageIdentity package)
        {
            var path = new PackagePath(
                Path.GetTempPath(),
                Path.Combine("folder", "src", "project1", "packages.config"),
                PackageReferenceType.PackagesConfig);
            return new PackageInProject(package.Id, package.Version.ToString(), path);
        }

        private static PackageUpdateSet UpdateSetFor(PackageIdentity package, params PackageInProject[] packages)
        {
            return UpdateSetFor(package, StandardPublishedDate, packages);
        }

        private static PackageUpdateSet UpdateSetFor(PackageIdentity package, DateTimeOffset published, params PackageInProject[] packages)
        {
            var latest = new PackageSearchMedatadata(package, "someSource", published, null);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, packages);
        }
    }
}