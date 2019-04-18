using System.Collections.Generic;
using System.Linq;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Inspection
{
    public static class PackagesFoundLogger
    {
        public static LogData Log(IReadOnlyCollection<PackageInProject> packages)
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
