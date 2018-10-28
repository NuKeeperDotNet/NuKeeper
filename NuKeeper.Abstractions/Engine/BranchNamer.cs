using System.Collections.Generic;
using System.Linq;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Abstractions.Engine
{
    public static class BranchNamer
    {
        public static string MakeName(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            return updates.Count > 1 ?
                MakeMultiPackageName(updates) :
                MakeSinglePackageName(updates.First());
        }

        private static string MakeMultiPackageName(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            var updatesHash = Hasher.Hash(PackageVersionStrings(updates));

            return $"nukeeper-update-{updates.Count}-packages-{updatesHash}";
        }

        public static string MakeSinglePackageName(PackageUpdateSet updateSet)
        {
            return $"nukeeper-update-{updateSet.SelectedId}-to-{updateSet.SelectedVersion}";
        }

        private static string PackageVersionStrings(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            return string.Join(",", updates.Select(PackageVersionString));
        }

        private static string PackageVersionString(PackageUpdateSet updateSet)
        {
            return $"{updateSet.SelectedId}-v{updateSet.SelectedVersion}";
        }
    }
}
