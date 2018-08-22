using System;
using System.Collections.Generic;
using System.Text;
using NSubstitute;
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

            var sorted = sorter.Sort(items);

            Assert.That(sorted, Is.Not.Null);
        }
    }
}
