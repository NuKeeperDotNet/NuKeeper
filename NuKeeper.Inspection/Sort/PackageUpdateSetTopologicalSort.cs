using System.Collections.Generic;
using System.Linq;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Sort
{
    public class PackageUpdateSetTopologicalSort : IPackageUpdateSetSort
    {
        private readonly INuKeeperLogger _logger;

        public PackageUpdateSetTopologicalSort(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<PackageUpdateSet> Sort(IReadOnlyCollection<PackageUpdateSet> input)
        {
            var topo = new TopologicalSort<PackageUpdateSet>(_logger, Match);

            var inputMap = input.Select(p =>
                new SortItemData<PackageUpdateSet>(p, PackageDeps(p, input)))
                .ToList();

            var sorted = topo.Sort(inputMap)
                .ToList();

            ReportSort(input.ToList(), sorted);
            return sorted;
        }

        private bool Match(PackageUpdateSet a, PackageUpdateSet b)
        {
            return a.SelectedId == b.SelectedId;
        }

        private static IReadOnlyCollection<PackageUpdateSet> PackageDeps(PackageUpdateSet set, IReadOnlyCollection<PackageUpdateSet> all)
        {
            var deps = set.Selected.Dependencies;
            return all.Where(i => deps.Any(d => d.Id == i.SelectedId)).ToList();
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
                    _logger.Detailed($"Resorted {output.Count} packages by dependencies, first change is {firstChange.SelectedId} moved to position {i} from {originalIndex}.");
                    break;
                }
            }

            if (!hasChange)
            {
                _logger.Detailed($"Sorted {output.Count} packages by dependencies but no change made");
            }
        }
    }
}
