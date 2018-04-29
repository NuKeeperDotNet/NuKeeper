using System.Collections.Generic;
using System.Linq;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Engine.Sort
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/Topological_sorting#Depth-first_search
    /// </summary>
    public class TopologicalSort
    {
        private readonly INuKeeperLogger _logger;
        private readonly List<PackageUpdateSet> _sortedList = new List<PackageUpdateSet>();
        private List<SortItemData> _data;
        private bool _cycleFound = false;

        public TopologicalSort(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public IList<PackageUpdateSet> Sort(IList<PackageUpdateSet> priorityOrder)
        {
            if (priorityOrder.Count < 2)
            {
                return priorityOrder;
            }

            _data = priorityOrder
                .Select(p => MakeNode(p, priorityOrder))
                .ToList();

            foreach (var item in _data)
            {
                if (item.Mark == Mark.None)
                {
                    Visit(item);
                }
            }

            if (_cycleFound)
            {
                return priorityOrder;
            }

            return _sortedList;
        }

        private void Visit(SortItemData item)
        {
            if (_cycleFound)
            {
                return;
            }

            if (item.Mark == Mark.Permanent)
            {
                return;
            }

            if (item.Mark == Mark.Temporary)
            {
                // cycle!
                _logger.Terse($"Cannot sort packages by dependency, cycle found at package {item.PackageId}");
                _cycleFound = true;
                return;
            }

            item.Mark = Mark.Temporary;
            var nodesDependedOn = item.Dependencies
                .Select(dep => _data.FirstOrDefault(i => i.PackageId == dep.Id))
                .Where(dep => dep != null);

            foreach (var dep in nodesDependedOn)
            {
                Visit(dep);
            }

            item.Mark = Mark.Permanent;
            _sortedList.Add(item.PackageUpdateSet);
        }

        private static SortItemData MakeNode(PackageUpdateSet set, IList<PackageUpdateSet> all)
        {
            var relevantDeps = set.Selected.Dependencies
                .Where(dep => all.Any(a => a.SelectedId == dep.Id));

            return new SortItemData(set, relevantDeps);
        }
    }
}
