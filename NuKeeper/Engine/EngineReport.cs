using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public static class EngineReport
    {
        public static string PackagesFound(List<PackageInProject> packages)
        {
            var projectPathCount = packages
                .Select(p => p.Path)
                .Distinct()
                .Count();

            var packageIds = packages
                .OrderBy(p => p.Id)
                .Select(p => p.Id)
                .Distinct()
                .ToList();

            return $"Found {packages.Count} packages in use, {packageIds.Count} distinct, in {projectPathCount} projects.\n{packageIds.JoinWithCommas()}";
        }

        public static string UpdatesFound(List<PackageUpdateSet> updates)
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine($"Found {updates.Count} possible updates:");

            foreach (var updateSet in updates)
            {
                foreach (var current in updateSet.CurrentPackages)
                {
                    result.AppendLine($"{updateSet.PackageId} from {current.Version} to {updateSet.NewVersion} in {current.Path.RelativePath}");
                }
            }

            return result.ToString();
        }

        public static string OldVersionsToBeUpdated(PackageUpdateSet updateSet)
        {
            var oldVersions = updateSet.CurrentPackages
                .Select(u => u.Version.ToString())
                .Distinct();

            return $"Updating '{updateSet.PackageId}' from {oldVersions.JoinWithCommas()} to {updateSet.NewVersion} in {updateSet.CurrentPackages.Count} projects";
        }
    }
}