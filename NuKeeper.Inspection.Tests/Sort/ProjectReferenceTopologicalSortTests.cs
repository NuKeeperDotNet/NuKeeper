using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using NSubstitute;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Inspection.Sort;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.Sort
{
    [TestFixture]
    public class ProjectReferenceTopologicalSortTests
    {
        [Test]
        public void CanSortEmptyList()
        {
            var dictionary = new SortedDictionary<string, IReadOnlyCollection<string>>();
            var readOnlyDictionary = new ReadOnlyDictionary<string, IReadOnlyCollection<string>>(dictionary);

            var sorter = new ProjectReferenceTopologicalSort(Substitute.For<INuKeeperLogger>());

            var sorted = sorter.Sort(readOnlyDictionary)
                .ToList();

            Assert.That(sorted, Is.Not.Null);
            Assert.That(sorted, Is.Empty);
        }

        [Test]
        public void CanSortOneItem()
        {
            var dictionary = new SortedDictionary<string, IReadOnlyCollection<string>>
            {
                { "a", new List<string>() }
            };

            var readOnlyDictionary = new ReadOnlyDictionary<string, IReadOnlyCollection<string>>(dictionary);

            var sorter = new ProjectReferenceTopologicalSort(Substitute.For<INuKeeperLogger>());

            var sorted = sorter.Sort(readOnlyDictionary)
                .ToList();

            AssertIsASortOf(sorted, readOnlyDictionary);
            Assert.That(sorted[0], Is.EqualTo(readOnlyDictionary.First().Key));
        }

        [Test]
        public void CanSortTwoUnrelatedItems()
        {
            var dictionary = new SortedDictionary<string, IReadOnlyCollection<string>>(new InverseStringComparer())
            {
                { "a", new List<string>() },
                { "b", new List<string>() }
            };

            var readOnlyDictionary = new ReadOnlyDictionary<string, IReadOnlyCollection<string>>(dictionary);

            var logger = Substitute.For<INuKeeperLogger>();

            var sorter = new ProjectReferenceTopologicalSort(logger);

            var sorted = sorter.Sort(readOnlyDictionary)
                .ToList();

            AssertIsASortOf(sorted, readOnlyDictionary);
            logger.Received(1).Detailed("No dependencies between items, no need to sort on dependencies");
        }

        [Test]
        public void CanSortTwoRelatedItemsinCorrectOrder()
        {
            var dictionary = new SortedDictionary<string, IReadOnlyCollection<string>>
            {
                { "a", new List<string>
                    {
                        "someproject{sep}someproject.csproj"
                    }
                }
                ,
                { "b", new List<string>
                    {
                        "someproject{sep}someotherproject.csproj",
                        "a"
                    }
                }
            };

            var readOnlyDictionary = new ReadOnlyDictionary<string, IReadOnlyCollection<string>>(dictionary);

            var logger = Substitute.For<INuKeeperLogger>();

            var sorter = new ProjectReferenceTopologicalSort(logger);

            var sorted = sorter.Sort(readOnlyDictionary)
                .ToList();

            AssertIsASortOf(sorted, readOnlyDictionary);

            logger.DidNotReceive().Detailed("No dependencies between items, no need to sort on dependencies");
            logger.Received(1).Detailed("Sorted 2 projects by project dependencies but no change made");
        }

        [Test]
        public void CanSortTwoRelatedItemsinReverseOrder()
        {
            const string projectAName = "a";
            const string projectBName = "b";

            var dictionary = new SortedDictionary<string, IReadOnlyCollection<string>>(new InverseStringComparer())
            {
                { projectBName, new List<string>
                    {
                        "someproject{sep}someotherproject.csproj",
                        projectAName
                    }
                },
                { projectAName, new List<string>
                    {
                        "someproject{sep}someproject.csproj"
                    }
                }
            };

            var readOnlyDictionary = new ReadOnlyDictionary<string, IReadOnlyCollection<string>>(dictionary);

            var logger = Substitute.For<INuKeeperLogger>();

            var sorter = new ProjectReferenceTopologicalSort(logger);

            var sorted = sorter.Sort(readOnlyDictionary)
                .ToList();

            AssertIsASortOf(sorted, readOnlyDictionary);

            Assert.That(sorted[0], Is.EqualTo(projectAName));
            Assert.That(sorted[1], Is.EqualTo(projectBName));

            logger.Received(1).Detailed(Arg.Is<string>(s =>
                s.StartsWith("Resorted 2 projects by project dependencies,", StringComparison.OrdinalIgnoreCase)));
        }

        [Test]
        public void CanSortFourRelatedItemsWithTransitiveProjectDependency()
        {
            const string projectAName = "a";
            const string transitiveProjectName = "transitive";
            const string anotherTransitiveProjectName = "anotherTransitive";
            const string projectBName = "b";

            var dictionary = new SortedDictionary<string, IReadOnlyCollection<string>>()
            {
                { projectBName, new List<string>
                    {
                        "someproject{sep}someotherproject.csproj",
                        anotherTransitiveProjectName
                    }
                },
                { projectAName, new List<string>
                    {
                        "someproject{sep}someproject.csproj"
                    }
                },
                {
                    transitiveProjectName, new List<string>
                    {
                        projectAName
                    }
                },
                {
                    anotherTransitiveProjectName, new List<string>
                    {
                        transitiveProjectName
                    }
                },
            };

            var readOnlyDictionary = new ReadOnlyDictionary<string, IReadOnlyCollection<string>>(dictionary);

            var logger = Substitute.For<INuKeeperLogger>();

            var sorter = new ProjectReferenceTopologicalSort(logger);

            var sorted = sorter.Sort(readOnlyDictionary)
                .ToList();

            AssertIsASortOf(sorted, readOnlyDictionary);

            Assert.That(sorted[0], Is.EqualTo(projectAName));
            Assert.That(sorted[1], Is.EqualTo(transitiveProjectName));
            Assert.That(sorted[2], Is.EqualTo(anotherTransitiveProjectName));
            Assert.That(sorted[3], Is.EqualTo(projectBName));

            logger.Received(1).Detailed(Arg.Is<string>(s =>
                s.StartsWith("Resorted 4 projects by project dependencies,", StringComparison.OrdinalIgnoreCase)));
        }

        [Test]
        public void CanSortWithCycle()
        {
            const string projectAName = "a";
            const string projectBName = "b";

            var dictionary = new SortedDictionary<string, IReadOnlyCollection<string>>(new InverseStringComparer())
            {
                { projectBName, new List<string>
                    {
                        "someproject{sep}someotherproject.csproj",
                        projectAName
                    }
                },
                { projectAName, new List<string>
                    {
                        "someproject{sep}someproject.csproj",
                        projectBName
                    }
                }
            };

            var readOnlyDictionary = new ReadOnlyDictionary<string, IReadOnlyCollection<string>>(dictionary);

            var logger = Substitute.For<INuKeeperLogger>();

            var sorter = new ProjectReferenceTopologicalSort(logger);

            var sorted = sorter.Sort(readOnlyDictionary)
                .ToList();

            AssertIsASortOf(sorted, readOnlyDictionary);

            logger.Received(1).Minimal(Arg.Is<string>(
                s => s.StartsWith("Cannot sort by dependencies, cycle found at item", StringComparison.OrdinalIgnoreCase)));
        }

        private static void AssertIsASortOf(List<string> sorted, IReadOnlyDictionary<string, IReadOnlyCollection<string>> original)
        {
            Assert.That(sorted, Is.Not.Null);
            Assert.That(sorted, Is.Not.Empty);
            Assert.That(sorted.Count, Is.EqualTo(original.Count));
            CollectionAssert.AreEquivalent(sorted, original.Keys);
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

            var packageVersionRange = PackageVersionRange.Parse(packageId, packageVersion);

            return new PackageInProject(packageVersionRange,
                new PackagePath(basePath, relativePath, PackageReferenceType.ProjectFile),
                refs);
        }

        private class InverseStringComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                var compare = string.Compare(x, y, StringComparison.OrdinalIgnoreCase);

                if (compare == 1)
                    return -1;

                if (compare == -1)
                    return 1;

                return compare;
            }
        }

    }
}
