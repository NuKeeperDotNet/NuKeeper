using System;
using System.Collections.Generic;
using System.Linq;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Engine
{
    public static class BranchNamer
    {        public static string MakeName(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            return updates.Count > 1 ?
                MakeMultiPackageName(updates.Count) :
                MakeSinglePackageName(updates.First());
        }

        private static string MakeMultiPackageName(int updateCount)
        {
            return $"nukeeper-update-{updateCount}-packages-{UniqueTimeString()}";
        }

        public static string MakeSinglePackageName(PackageUpdateSet updateSet)
        {
            return $"nukeeper-update-{updateSet.SelectedId}-to-{updateSet.SelectedVersion}";
        }

        private static string UniqueTimeString()
        {
            var now = DateTime.UtcNow;
            return $"{now.Year}{now.Month:00}{now.Day:00}T{now.Hour:00}{now.Minute:00}{now.Second:00}";
        }
    }
}
