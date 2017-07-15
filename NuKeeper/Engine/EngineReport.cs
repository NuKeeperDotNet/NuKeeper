using System;
using System.Collections.Generic;
using System.Linq;
using NuKeeper.Nuget.Api;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public static class EngineReport
    {
        public static void PackagesFound(List<PackageInProject> packages)
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

            Console.WriteLine($"Found {packages.Count} packages in use, {packageIds.Count} distinct, in {projectPathCount} projects.");

            var packageIdsText = string.Join(", ", packageIds);
            Console.WriteLine(packageIdsText);
        }

        public static void UpdatesFound(List<PackageUpdateSet> updates)
        {
            Console.WriteLine($"Found {updates.Count} possible updates:");

            foreach (var updateSet in updates)
            {
                foreach (var current in updateSet.CurrentPackages)
                {
                    Console.WriteLine($"{updateSet.PackageId} from {current.Version} to {updateSet.NewVersion} in {current.Path.RelativePath}");
                }
            }
        }
    }
}