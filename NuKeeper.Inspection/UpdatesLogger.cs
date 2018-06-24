using System.Collections.Generic;
using System.Text;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection
{
    public static class UpdatesLogger
    {
        public static LogData Log(IReadOnlyCollection<PackageUpdateSet> updates)
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
    }
}
