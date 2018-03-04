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
            return update.CountCurrentVersions();
        }
    }
}
