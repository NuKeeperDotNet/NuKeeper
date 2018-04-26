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
            var depIndex = IndexOfAnyDependency(rest, first.Selected.Dependencies);

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
            List<PackageUpdateSet> sets,
            IReadOnlyCollection<PackageDependency> dependencies)
        {
            var checkDeps = dependencies
                .Select(d => d.Id)
                .ToList();

            for (int i = 0; i < sets.Count; i++)
            {
                var item = sets[i];

                if (checkDeps.Any(d => d == item.SelectedId))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
