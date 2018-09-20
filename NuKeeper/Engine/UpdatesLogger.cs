using System;
using System.Collections.Generic;
using System.Linq;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Engine
{
    public static class UpdatesLogger
    {
        public static string OldVersionsToBeUpdated(PackageUpdateSet updateSet)
        {
            var oldVersions = updateSet.CurrentPackages
                .Select(u => u.Version.ToString())
                .Distinct();

            return $"Updating '{updateSet.SelectedId}' from {oldVersions.JoinWithCommas()} to {updateSet.SelectedVersion} in {updateSet.CurrentPackages.Count} projects";
        }

        public static string OldVersionsToBeUpdated(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            return updates.Select(OldVersionsToBeUpdated).JoinWithSeparator(Environment.NewLine);
        }
    }
}
