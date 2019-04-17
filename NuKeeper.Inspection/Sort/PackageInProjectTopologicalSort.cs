using System.Collections.Generic;
using System.Linq;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Inspection.Sort
{
    public class PackageInProjectTopologicalSort
    {
        private readonly INuKeeperLogger _logger;

        public PackageInProjectTopologicalSort(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<PackageInProject> Sort(IReadOnlyCollection<PackageInProject> input)
        {
            var topo = new TopologicalSort<PackageInProject>(_logger, Match);

            var inputMap = input.Select(p =>
                    new SortItemData<PackageInProject>(p, ProjectDeps(p, input)))
                .ToList();

            var sorted = topo.Sort(inputMap)
                .ToList();

            sorted.Reverse();

            ReportSort(input.ToList(), sorted);

            return sorted;
        }

        private bool Match(PackageInProject a, PackageInProject b)
        {
            return a.Path.FullName == b.Path.FullName;
        }

        private static IReadOnlyCollection<PackageInProject> ProjectDeps(PackageInProject selected,
            IReadOnlyCollection<PackageInProject> all)
        {
            var deps = selected.ProjectReferences;
            return all.Where(i => deps.Any(d => d == i.Path.FullName)).ToList();
        }

        private void ReportSort(IList<PackageInProject> input, IList<PackageInProject> output)
        {
            bool hasChange = false;

            for (int i = 0; i < output.Count; i++)
            {
                if (input[i] != output[i])
                {
                    hasChange = true;
                    var firstChange = output[i];
                    var originalIndex = input.IndexOf(firstChange);
                    _logger.Detailed($"Resorted {output.Count} projects by dependencies, first change is {firstChange.Path.RelativePath} moved to position {i} from {originalIndex}.");
                    break;
                }
            }

            if (!hasChange)
            {
                _logger.Detailed($"Sorted {output.Count} projects by dependencies but no change made");
            }
        }
    }
}
