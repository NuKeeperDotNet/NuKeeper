using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuKeeper.Engine;
using NuKeeper.Inspection.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class UpdateConsolidatorTests
    {
        [Test]
        public void WhenItemsAreConsolidated()
        {
            var items = new List<PackageUpdateSet>
            {
                PackageUpdates.MakeUpdateSet("foo"),
                PackageUpdates.MakeUpdateSet("bar")
            };

            var output = UpdateConsolidator.Consolidate(items, true);

            var listOfLists = output.ToList();

            // one list, containing all the items
            Assert.That(listOfLists.Count, Is.EqualTo(1));
            Assert.That(listOfLists[0].Count, Is.EqualTo(2));
        }

        [Test]
        public void WhenItemsAreNotConsolidated()
        {
            var items = new List<PackageUpdateSet>
            {
                PackageUpdates.MakeUpdateSet("foo"),
                PackageUpdates.MakeUpdateSet("bar")
            };

            var output = UpdateConsolidator.Consolidate(items, false);

            var listOfLists = output.ToList();

            // two lists, each containing 1 item
            Assert.That(listOfLists.Count, Is.EqualTo(2));
            Assert.That(listOfLists.SelectMany(x => x).Count(), Is.EqualTo(2));
            Assert.That(listOfLists[0].Count, Is.EqualTo(1));
            Assert.That(listOfLists[1].Count, Is.EqualTo(1));
        }
    }
}
