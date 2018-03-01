using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuKeeper.Logging;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public static class UpdatesLogger
    {
        public static LogData PackagesFound(List<PackageInProject> packages)
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

            var headline = $"Found {packages.Count} packages in use, {packageIds.Count} distinct, in {projectPathCount} projects.";

            return new LogData
            {
                Terse = headline,
                Info = packageIds.JoinWithCommas()
            };
        }

        public static LogData UpdatesFound(List<PackageUpdateSet> updates)
        {
            var headline = $"Found {updates.Count} possible updates";
            var details = new StringBuilder();

            foreach (var updateSet in updates)
            {
                foreach (var current in updateSet.CurrentPackages)
                {
                    details.AppendLine($"{updateSet.SelectedId} from {current.Version} to {updateSet.SelectedVersion} in {current.Path.RelativePath}");
                }
            }
            return new LogData
            {
                Terse = headline,
                Info = details.ToString()
            };
        }

        public static string OldVersionsToBeUpdated(PackageUpdateSet updateSet)
        {
            var oldVersions = updateSet.CurrentPackages
                .Select(u => u.Version.ToString())
                .Distinct();

            return $"Updating '{updateSet.SelectedId}' from {oldVersions.JoinWithCommas()} to {updateSet.SelectedVersion} in {updateSet.CurrentPackages.Count} projects";
        }
    }
}
