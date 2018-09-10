using System.Collections.Generic;
using System.Linq;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Engine
{
    public static class BranchNamer
    {
        public static string MakeName(PackageUpdateSet updateSet)
        {
            return $"nukeeper-update-{updateSet.SelectedId}-to-{updateSet.SelectedVersion}";
        }

        public static string MakeName(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            if (updates.Count == 1)
            {
                return MakeName(updates.First());
            }

            // TODO: Complete - add timestamp
            return "nukeeper-update";
        }
    }
}
