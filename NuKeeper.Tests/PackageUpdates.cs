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
    public static class PackageUpdates
    {
        public static PackageUpdateSet MakeUpdateSet(string packageName, string version = "1.2.3")
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

        public static PackageUpdateSet For(params PackageInProject[] packages)
        {
            var newPackage = new PackageIdentity("foo.bar", new NuGetVersion("1.2.3"));
            return ForNewVersion(newPackage, packages);
        }

        public static PackageUpdateSet ForNewVersion(PackageIdentity newPackage, params PackageInProject[] packages)
        {
            var publishedDate = new DateTimeOffset(2018, 2, 19, 11, 12, 7, TimeSpan.Zero);
            var latest = new PackageSearchMedatadata(newPackage, OfficialPackageSource(), publishedDate, null);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, packages);
        }

        public static PackageUpdateSet ForInternalSource(params PackageInProject[] packages)
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
