using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Inspection.Tests.Report
{
    public static class PackageUpdates
    {
        public static PackageUpdateSet UpdateSetFor(PackageIdentity package, params PackageInProject[] packages)
        {
            var publishedDate = new DateTimeOffset(2018, 2, 19, 11, 12, 7, TimeSpan.Zero);
            var latest = new PackageSearchMedatadata(package, new PackageSource("http://none"), publishedDate, null);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, packages);
        }

        public static PackageInProject MakePackageForV110(PackageIdentity package)
        {
            var path = new PackagePath(
                OsSpecifics.GenerateBaseDirectory(),
                Path.Combine("folder", "src", "project1", "packages.config"),
                PackageReferenceType.PackagesConfig);
            return new PackageInProject(package, path);
        }

        internal static List<PackageUpdateSet> PackageUpdateSets(int count)
        {
            var result = new List<PackageUpdateSet>();
            foreach (var index in Enumerable.Range(1, count))
            {
                var package = new PackageIdentity($"test.package{index}",
                    new NuGetVersion($"1.2.{index}"));

                var updateSet = UpdateSetFor(package, MakePackageForV110(package));
                result.Add(updateSet);
            }

            return result;
        }

        public static List<PackageUpdateSet> OnePackageUpdateSet()
        {
            var package = new PackageIdentity("foo.bar", new NuGetVersion("1.2.3"));

            return new List<PackageUpdateSet>
            {
                UpdateSetFor(package, MakePackageForV110(package))
            };
        }
    }
}
