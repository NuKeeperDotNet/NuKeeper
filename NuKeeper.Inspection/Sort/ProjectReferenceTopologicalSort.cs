using System.Collections.Generic;
using System.Linq;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.Inspection.Sort
{
    public class ProjectReferenceTopologicalSort
    {
        private readonly INuKeeperLogger _logger;

        public ProjectReferenceTopologicalSort(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<string> Sort(IReadOnlyDictionary<string, IReadOnlyCollection<string>> input)
        {
            var topo = new TopologicalSort<string>(_logger, Match);

            var inputMap = input.Select(p =>
                    new SortItemData<string>(p.Key, GetProjectReferences(p.Key, input)))
                .ToList();

            var sorted = topo.Sort(inputMap)
                .ToList();

            ReportSort(inputMap.Select(m => m.Item).ToList(), sorted);

            return sorted;
        }

        private bool Match(string projectPathA, string projectPathB)
        {
            return projectPathA == projectPathB;
        }

        private static IEnumerable<string> GetProjectReferences(string project, IReadOnlyDictionary<string, IReadOnlyCollection<string>> projectReferenceMap)
        {
            if (!projectReferenceMap.ContainsKey(project))
                return Enumerable.Empty<string>();

            return projectReferenceMap[project];
        }

        private void ReportSort(IList<string> input, IList<string> output)
        {
            bool hasChange = false;

            for (int i = 0; i < output.Count; i++)
            {
                if (input[i] != output[i])
                {
                    hasChange = true;
                    var firstChange = output[i];
                    var originalIndex = input.IndexOf(firstChange);
                    _logger.Detailed($"Resorted {output.Count} projects by project dependencies, first change is {firstChange} moved to position {i} from {originalIndex}.");
                    break;
                }
            }

            if (!hasChange)
            {
                _logger.Detailed($"Sorted {output.Count} projects by project dependencies but no change made");
            }
        }
    }
}
