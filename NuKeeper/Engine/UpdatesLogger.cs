using System.Linq;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Types.Formats;

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
    }
}
