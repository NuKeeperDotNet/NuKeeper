using System.Collections.Generic;
using System.Linq;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Engine
{
    public static class UpdateConsolidator
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
