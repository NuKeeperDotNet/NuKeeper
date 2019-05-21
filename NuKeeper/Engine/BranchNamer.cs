using System.Collections.Generic;
using System.Linq;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Engine
{
    public static class BranchNamer
    {
        public static string MakeName(IReadOnlyCollection<PackageUpdateSet> updates, string branchPrefixName = null)
        {
            return updates.Count > 1 ?
                MakeMultiPackageName(updates, branchPrefixName) :
                MakeSinglePackageName(updates.First(), branchPrefixName);
        }

        public static string MakeSinglePackageName(PackageUpdateSet updateSet, string branchPrefixName = null)
        {
            return $"{branchPrefixName}nukeeper-update-{updateSet.SelectedId}-to-{updateSet.SelectedVersion}";
        }

        private static string MakeMultiPackageName(IReadOnlyCollection<PackageUpdateSet> updates, string branchNamePrefix)
        {
            var updatesHash = Hasher.Hash(PackageVersionStrings(updates));

            return $"{branchNamePrefix}nukeeper-update-{updates.Count}-packages-{updatesHash}";
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
