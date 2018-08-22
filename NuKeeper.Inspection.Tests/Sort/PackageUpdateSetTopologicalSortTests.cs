using System.Collections.Generic;
using NSubstitute;
using NuKeeper.Inspection.Logging;
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

            var sorted = sorter.Sort(items);

            Assert.That(sorted, Is.Not.Null);
        }
    }
}
