using System.Collections.Generic;
using System.Linq;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Sort
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
            var prioritySorter = new PrioritySort();
            var topoSorter = new TopologicalSort(_logger);

            var priorityOrder = prioritySorter.Sort(input);
            return topoSorter.Sort(priorityOrder.ToList());
        }
    }
}
