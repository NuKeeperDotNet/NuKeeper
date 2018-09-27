using System.Collections.Generic;
using System.Linq;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Engine
{
    public static class PackageUpdateConsolidator
    {
        public static IReadOnlyCollection<IReadOnlyCollection<PackageUpdateSet>> Consolidate(
            IReadOnlyCollection<PackageUpdateSet> updates, bool consolidate)
        {
            if (consolidate)
            {
                return new List<IReadOnlyCollection<PackageUpdateSet>> { updates };
            }

            return updates.Select(u => new List<PackageUpdateSet> { u }).ToList();
        }
    }
}
