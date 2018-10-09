using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                PackageFor("foo", "1.2.3", "bar{sep}fish.csproj"),
            };

            var sorter = new PackageInProjectTopologicalSort(Substitute.For<INuKeeperLogger>());

            var sorted = sorter.Sort(items)
                .ToList();

            AssertIsASortOf(sorted, items);
            Assert.That(sorted[0], Is.EqualTo(items[0]));
        }

        [Test]
        public void CanSortTwoUnrelatedItems()
        {
            var items = new List<PackageInProject>
            {
                PackageFor("foo", "1.2.3", "bar{sep}fish.csproj"),
                PackageFor("bar", "2.3.4", "project2{sep}p2.csproj")
            };

            var logger = Substitute.For<INuKeeperLogger>();

            var sorter = new PackageInProjectTopologicalSort(logger);

            var sorted = sorter.Sort(items)
                .ToList();

            AssertIsASortOf(sorted, items);
            logger.Received(1).Detailed("No dependencies between items, no need to sort on dependencies");
       }

        [Test]
        public void CanSortTwoRelatedItemsinCorrectOrder()
        {
            var aProj = PackageFor("foo", "1.2.3", "someproject{sep}someproject.csproj");
            var testProj = PackageFor("bar", "2.3.4", "someproject.tests{sep}someproject.tests.csproj", aProj);

            var items = new List<PackageInProject>
            {
                testProj,
                aProj
            };

            var logger = Substitute.For<INuKeeperLogger>();

            var sorter = new PackageInProjectTopologicalSort(logger);

            var sorted = sorter.Sort(items)
                .ToList();

            AssertIsASortOf(sorted, items);
            Assert.That(sorted[0], Is.EqualTo(items[0]));
            Assert.That(sorted[1], Is.EqualTo(items[1]));

            logger.DidNotReceive().Detailed("No dependencies between items, no need to sort on dependencies");
            logger.Received(1).Detailed("Sorted 2 projects by dependencies but no change made");
        }

        [Test]
        public void CanSortTwoRelatedItemsinReverseOrder()
        {
            var aProj = PackageFor("foo", "1.2.3", "someproject{sep}someproject.csproj");
            var testProj = PackageFor("bar", "2.3.4", "someproject.tests{sep}someproject.tests.csproj", aProj);

            var items = new List<PackageInProject>
            {
                aProj,
                testProj
            };

            var logger = Substitute.For<INuKeeperLogger>();

            var sorter = new PackageInProjectTopologicalSort(logger);

            var sorted = sorter.Sort(items)
                .ToList();

            AssertIsASortOf(sorted, items);
            Assert.That(sorted[0], Is.EqualTo(testProj));
            Assert.That(sorted[1], Is.EqualTo(aProj));

            logger.Received(1).Detailed(Arg.Is<string>(s => s.StartsWith("Resorted 2 projects by dependencies,")));
        }

        [Test]
        public void CanSortWithCycle()
        {
            var aProj = PackageFor("foo", "1.2.3", "someproject{sep}someproject.csproj");
            var testProj = PackageFor("bar", "2.3.4", "someproject.tests{sep}someproject.tests.csproj", aProj);
            // fake a circular ref - aproj is a new object but the same file path as above
            aProj = PackageFor("foo", "1.2.3", "someproject{sep}someproject.csproj", testProj);

            var items = new List<PackageInProject>
            {
                aProj,
                testProj
            };

            var logger = Substitute.For<INuKeeperLogger>();

            var sorter = new PackageInProjectTopologicalSort(logger);

            var sorted = sorter.Sort(items)
                .ToList();

            AssertIsASortOf(sorted, items);
            logger.Received(1).Minimal(Arg.Is<string>(s => s.StartsWith("Cannot sort by dependencies, cycle found at item")));
        }

        private static void AssertIsASortOf(List<PackageInProject> sorted, List<PackageInProject> original)
        {
            Assert.That(sorted, Is.Not.Null);
            Assert.That(sorted, Is.Not.Empty);
            Assert.That(sorted.Count, Is.EqualTo(original.Count));
            CollectionAssert.AreEquivalent(sorted, original);
        }

        private static PackageInProject PackageFor(string packageId, string packageVersion,
            string relativePath, PackageInProject refProject = null)
        {
            relativePath = relativePath.Replace("{sep}", $"{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
            var basePath = "c_temp" + Path.DirectorySeparatorChar + "test";

            var refs = new List<string>();

            if (refProject != null)
            {
                refs.Add(refProject.Path.FullName);
            }

            return new PackageInProject(
                new PackageIdentity(packageId, new NuGetVersion(packageVersion)),
                new PackagePath(basePath, relativePath, PackageReferenceType.ProjectFile),
                refs);
        }
    }
}
