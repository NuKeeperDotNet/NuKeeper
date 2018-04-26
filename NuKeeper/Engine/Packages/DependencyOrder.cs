using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging.Core;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Engine.Packages
{
    public static class DependencyOrder
    {
        public static IEnumerable<PackageUpdateSet> Sort(IList<PackageUpdateSet> priorityOrder)
        {
            if (priorityOrder.Count < 2)
            {
                return priorityOrder;
            }

            var first = priorityOrder.First();
            var rest = priorityOrder.Skip(1).ToList();
            var depIndex = IndexOfAnyDependency(first.Selected.Dependencies, rest);

            if (depIndex == -1)
            {
                return new List<PackageUpdateSet> { first }
                    .Concat(Sort(rest))
                    .ToList();
            }

            rest.Insert(depIndex + 1, first);
            return Sort(rest);
        }

        private static int IndexOfAnyDependency(
            IReadOnlyCollection<PackageDependency> dependencies,
            List<PackageUpdateSet> sets)
        {
            var targetDependencyIds = dependencies
                .Select(d => d.Id)
                .ToList();

            for (var i = 0; i < sets.Count; i++)
            {
                var testId = sets[i].SelectedId;

                if (targetDependencyIds.Any(d => d == testId))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
