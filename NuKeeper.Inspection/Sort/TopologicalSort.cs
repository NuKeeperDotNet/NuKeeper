using System.Collections.Generic;
using System.Linq;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Sort
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/Topological_sorting#Depth-first_search
    /// </summary>
    public class TopologicalSort : IPackageUpdateSetSort
    {
        private readonly INuKeeperLogger _logger;
        private readonly List<PackageUpdateSet> _sortedList = new List<PackageUpdateSet>();
        private List<SortItemData> _data;
        private bool _cycleFound;

        public TopologicalSort(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<PackageUpdateSet> Sort(IReadOnlyCollection<PackageUpdateSet> input)
        {
            if (input.Count < 2)
            {
                return input;
            }

            _data = input
                .Select(p => MakeNode(p, input))
                .ToList();

            if (!_data.Any(i => i.Dependencies.Any()))
            {
                _logger.Detailed("No dependencies between packages being updated, no need to sort on this");
                return input;
            }

            foreach (var item in _data)
            {
                if (item.Mark == Mark.None)
                {
                    Visit(item);
                }
            }

            if (_cycleFound)
            {
                return input;
            }

            ReportSort(input.ToList(), _sortedList);
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
                _logger.Minimal($"Cannot sort packages by dependencies, cycle found at package {item.PackageId}");
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

        private static SortItemData MakeNode(PackageUpdateSet set, IEnumerable<PackageUpdateSet> all)
        {
            var relevantDeps = set.Selected.Dependencies
                .Where(dep => all.Any(a => a.SelectedId == dep.Id));

            return new SortItemData(set, relevantDeps);
        }

        private void ReportSort(IList<PackageUpdateSet> input, IList<PackageUpdateSet> output)
        {
            bool hasChange = false;

            for (int i = 0; i < output.Count; i++)
            {
                if (input[i] != output[i])
                {
                    hasChange = true;
                    var firstChange = output[i];
                    var originalIndex = input.IndexOf(firstChange);
                    _logger.Detailed($"Resorted packages by dependencies, first change is {firstChange.SelectedId} moved to position {i} from {originalIndex}.");
                    break;
                }
            }

            if (!hasChange)
            {
                _logger.Detailed("Sorted packages by dependencies but no change made");
            }
        }
    }
}
