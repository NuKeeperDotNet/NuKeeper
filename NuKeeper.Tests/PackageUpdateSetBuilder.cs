using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Tests
{
    public static class PackageUpdateSetBuilder
    {
        public static List<T> InList<T>(this T item)
        {
            return new List<T>
            {
                item
            };
        }

        public static PackageUpdateSet MakePackageUpdateSet(string packageName, string version = "1.2.3")
        {
            var packageId = new PackageIdentity(packageName, new NuGetVersion(version));
            var latest = new PackageSearchMedatadata(
                packageId, OfficialPackageSource(),
                null,
                Enumerable.Empty<PackageDependency>());

            var packages = new PackageLookupResult(VersionChange.Major, latest, null, null);

            var path = new PackagePath("c:\\foo", "bar", PackageReferenceType.ProjectFile);
            var pip = new PackageInProject(packageId, path, null);

            return new PackageUpdateSet(packages, new List<PackageInProject> { pip });
        }

        public static PackageUpdateSet UpdateSetFor(params PackageInProject[] packages)
        {
            var newPackage = new PackageIdentity("foo.bar", new NuGetVersion("1.2.3"));
            return UpdateSetForNewVersion(newPackage, packages);
        }

        public static PackageUpdateSet UpdateSetForNewVersion(PackageIdentity newPackage, params PackageInProject[] packages)
        {
            var publishedDate = new DateTimeOffset(2018, 2, 19, 11, 12, 7, TimeSpan.Zero);
            var latest = new PackageSearchMedatadata(newPackage, OfficialPackageSource(), publishedDate, null);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, packages);
        }

        public static PackageUpdateSet UpdateSetForInternalSource(params PackageInProject[] packages)
        {
            var newPackage = new PackageIdentity("foo.bar", new NuGetVersion("1.2.3"));
            var publishedDate = new DateTimeOffset(2018, 2, 19, 11, 12, 7, TimeSpan.Zero);
            var latest = new PackageSearchMedatadata(newPackage,
                InternalPackageSource(), publishedDate, null);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, packages);
        }

        public static PackageSource OfficialPackageSource()
        {
            return new PackageSource(NuGetConstants.V3FeedUrl);
        }

        public static PackageSource InternalPackageSource()
        {
            return new PackageSource("http://internalfeed.myco.com/api");
        }
    }
}
