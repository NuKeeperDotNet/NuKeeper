using NuGet.Versioning;
using NuKeeper.RepositoryInspection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuKeeper.Engine.Packages
{
    public static class PackageUpdateSort
    {
        private const long Shift = 1000;

        public static IEnumerable<PackageUpdateSet> Sort(IEnumerable<PackageUpdateSet> packages)
        {
            return packages.OrderByDescending(Priority);
        }

        private static long Priority(PackageUpdateSet update)
        {
            long countCurrentVersions = update.CountCurrentVersions();
            long countUsages = update.CurrentPackages.Count;
            var versionChangeScore = ScoreVersionChange(update);
            var ageScore = ScoreAge(update);

            long score = countCurrentVersions;
            score = score * Shift;
            score = score + countUsages;
            score = score * Shift;
            score = score + versionChangeScore + ageScore;
            return score;
        }

        private static long ScoreAge(PackageUpdateSet update)
        {
            var publishedDate = update.Selected.Published;
            if (!publishedDate.HasValue)
            {
                return 0;
            }

            var published = publishedDate.Value.ToUniversalTime().DateTime;
            var interval = DateTime.UtcNow.Subtract(published);
            return interval.Days;
        }

        private static long ScoreVersionChange(PackageUpdateSet update)
        {
            var newVersion = update.Selected.Identity.Version;
            var versionInUse = update.CurrentPackages
                .Select(p => p.Version)
                .Max();

            return ScoreVersionChange(newVersion, versionInUse);
        }

        private static long ScoreVersionChange(NuGetVersion newVersion, NuGetVersion oldVersion)
        {
            long preReleaseScore = 0;
            if (oldVersion.IsPrerelease && !newVersion.IsPrerelease)
            {
                preReleaseScore = Shift * 10;
            }

            var majors = newVersion.Major - oldVersion.Major;
            if (majors > 0)
            {
                return (majors * 100) + preReleaseScore;
            }

            var minors = newVersion.Minor - oldVersion.Minor;
            if (minors > 0)
            {
                return (minors * 10) + preReleaseScore;
            }

            var patches = newVersion.Patch - oldVersion.Patch;
            if (patches > 0)
            {
                return patches + preReleaseScore;
            }

            return preReleaseScore;
        }
    }
}
