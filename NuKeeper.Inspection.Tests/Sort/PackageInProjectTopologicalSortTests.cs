using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSubstitute;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sort;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.Sort
{
    [TestFixture]
    public class PackageInProjectTopologicalSortTests
    {
        [Test]
        public void CanSortEmptyList()
        {
            var items = new List<PackageInProject>();

            var sorter = new PackageInProjectTopologicalSort(Substitute.For<INuKeeperLogger>());

            var sorted = sorter.Sort(items)
                .ToList();

            Assert.That(sorted, Is.Not.Null);
            Assert.That(sorted, Is.Empty);
        }

        [Test]
        public void CanSortOneItem()
        {
            var items = new List<PackageInProject>
            {
                new PackageInProject(new PackageIdentity("foo", new NuGetVersion("1.2.3")),
                    new PackagePath("c:\\foo", "\\bar\fish.csproj", PackageReferenceType.ProjectFile),
                    null)
            };

            var sorter = new PackageInProjectTopologicalSort(Substitute.For<INuKeeperLogger>());

            var sorted = sorter.Sort(items)
                .ToList();

            Assert.That(sorted, Is.Not.Null);
            Assert.That(sorted, Is.Not.Empty);
            Assert.That(sorted[0], Is.EqualTo(items[0]));
        }

        [Test]
        public void CanSortTwoUnrelatedItems()
        {
            var items = new List<PackageInProject>
            {
                PackageFor("foo", "1.2.3", "\\bar\\fish.csproj"),
                PackageFor("bar", "2.3.4", "\\project2\\p2.csproj")
            };

            var logger = Substitute.For<INuKeeperLogger>();

            var sorter = new PackageInProjectTopologicalSort(logger);

            var sorted = sorter.Sort(items)
                .ToList();

            Assert.That(sorted, Is.Not.Null);
            Assert.That(sorted, Is.Not.Empty);
            Assert.That(sorted[0], Is.EqualTo(items[0]));
            Assert.That(sorted[1], Is.EqualTo(items[1]));

            logger.Received(1).Detailed("No dependencies between items, no need to sort on dependencies");
            logger.Received(1).Detailed("Sorted 2 projects by dependencies but no change made");
        }

        private PackageInProject PackageFor(string packageId, string packageVersion,
            string relativePath)
        {
            return new PackageInProject(
                new PackageIdentity(packageId, new NuGetVersion(packageVersion)),
                new PackagePath("c_temp\\foo", relativePath, PackageReferenceType.ProjectFile),
                null);
        }
    }
}
