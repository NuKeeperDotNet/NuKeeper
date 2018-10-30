using System;
using System.Collections.Generic;
using System.Linq;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Engine
{
    public static class UpdatesLogger
    {
        public static string OldVersionsToBeUpdated(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            if (updates.Count == 1)
            {
                return "Updating" + DescribeOldVersions(updates.First());
            }

            return $"Updating {updates.Count} packages" + Environment.NewLine +
                updates.Select(DescribeOldVersions).JoinWithSeparator(Environment.NewLine);
        }

        private static string DescribeOldVersions(PackageUpdateSet updateSet)
        {
            var oldVersions = updateSet.CurrentPackages
                .Select(u => u.Version.ToString())
                .Distinct();

            return $"'{updateSet.SelectedId}' from {oldVersions.JoinWithCommas()} to {updateSet.SelectedVersion} in {updateSet.CurrentPackages.Count} projects";
        }
    }
}
