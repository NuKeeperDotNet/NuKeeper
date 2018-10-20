using System.Collections.Generic;
using System.Linq;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Abstract.Engine
{
    public static class BranchNamer
    {
        public static string MakeName(PackageUpdateSet updateSet)
        {
            return $"nukeeper-update-{updateSet.SelectedId}-to-{updateSet.SelectedVersion}";
        }

        public static string MakeName(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            return updates.Count > 1 ? "nukeeper-update-packages" : MakeName(updates.First());
        }
    }
}
