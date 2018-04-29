using System.Collections.Generic;
using System.Linq;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Engine.Sort
{
    public class PackageUpdateSetSort : IPackageUpdateSetSort
    {
        private readonly INuKeeperLogger _logger;

        public PackageUpdateSetSort(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<PackageUpdateSet> Sort(IReadOnlyCollection<PackageUpdateSet> input)
        {
            var priorityOrder = PrioritySort.Sort(input);
            var depSorter = new TopologicalSort(_logger);
            return depSorter.Sort(priorityOrder.ToList());
        }
    }
}
