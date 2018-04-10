using System.Collections.Generic;
using System.Linq;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Types.Logging;

namespace NuKeeper.Inspection
{
    public static class PackagesFoundLogger
    {
        public static LogData Log(List<PackageInProject> packages)
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
    }
}
