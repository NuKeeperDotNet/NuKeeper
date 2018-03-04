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

        private static long Priority(PackageUpdateSet update)
        {

            const long Shift = 1000;
            long score = update.CountCurrentVersions();
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
            var majors = newVersion.Major - oldVersion.Major;
            if (majors > 0)
            {
                return majors * 100;
            }

            var minors = newVersion.Minor - oldVersion.Minor;
            if (minors > 0)
            {
                return minors * 10;
            }

            var patches = newVersion.Patch - oldVersion.Patch;
            if (patches > 0)
            {
                return patches;
            }

            return 0;
        }
    }
}
