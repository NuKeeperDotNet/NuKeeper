using System;
using System.Collections.Generic;
using System.Linq;
using NuKeeper.NuGet.Api;
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

        public static void UpdatesFound(List<PackageUpdate> updates)
        {
            Console.WriteLine($"Found {updates.Count} possible updates:");

            foreach (var up in updates)
            {
                Console.WriteLine($"{up.PackageId} from {up.OldVersion} to {up.NewVersion} in {up.CurrentPackage.Path.RelativePath}");
            }
        }
    }
}