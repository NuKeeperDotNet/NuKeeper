using NuGet.Versioning;
using NuKeeper.RepositoryInspection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuKeeper.Engine.Packages
{
    public static class PackageUpdateSort
    {
        public static IEnumerable<PackageUpdateSet> Sort(IEnumerable<PackageUpdateSet> packages)
        {
            return packages.OrderByDescending(Priority);
        }

        private static int Priority(PackageUpdateSet update)
        {

            const int Shift = 100;
            var score = update.CountCurrentVersions();
            score = score * Shift;
            score = score + update.CurrentPackages.Count;
            score = score * Shift;

            var newVersion = update.Selected.Identity.Version;
            var versionInUse = update.CurrentPackages
                .Select(p => p.Version)
                .Max();
            score = score + ScoreVersionChange(newVersion, versionInUse);
            return score;
        }

        private static int ScoreVersionChange(NuGetVersion newVersion, NuGetVersion oldVersion)
        {
            return 0;
        }
    }
}
