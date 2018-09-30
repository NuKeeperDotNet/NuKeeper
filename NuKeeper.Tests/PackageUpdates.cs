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
        private static readonly DateTimeOffset StandardPublishedDate = new DateTimeOffset(2018, 2, 19, 11, 12, 7, TimeSpan.Zero);

        public static PackageUpdateSet UpdateSet()
        {
            var fooPackage = new PackageIdentity("foo", new NuGetVersion(1, 2, 3));
            var path = new PackagePath("c:\\foo", "bar", PackageReferenceType.PackagesConfig);
            var packages = new[]
            {
                new PackageInProject(fooPackage, path, null)
            };

            var publishedDate = new DateTimeOffset(2018, 2, 19, 11, 12, 7, TimeSpan.Zero);
            var latest = new PackageSearchMedatadata(fooPackage,
                OfficialPackageSource(), publishedDate, null);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, packages);
        }

        public static PackageUpdateSet For(
            PackageIdentity package,
            DateTimeOffset published,
            List<PackageInProject> packages,
            List<PackageDependency> dependencies)
        {
            var latest = new PackageSearchMedatadata(package, OfficialPackageSource(), published, dependencies);
            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, packages);
        }

        public static PackageUpdateSet ForRefType(PackageReferenceType refType)
        {
            var fooPackage = new PackageIdentity("foo", new NuGetVersion(1, 2, 3));
            var path = new PackagePath("c:\\foo", "bar", refType);
            var packages = new[]
            {
                new PackageInProject(fooPackage, path, null)
            };

            var latest = new PackageSearchMedatadata(fooPackage, OfficialPackageSource(), null, null);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, packages);
        }


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

        public static PackageUpdateSet UpdateSetFor(PackageIdentity package, params PackageInProject[] packages)
        {
            return UpdateSetFor(package, StandardPublishedDate, packages);
        }

        public static PackageUpdateSet UpdateSetFor(PackageIdentity package, DateTimeOffset published, params PackageInProject[] packages)
        {
            var latest = new PackageSearchMedatadata(package, OfficialPackageSource(), published, null);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, packages);
        }

        public static PackageUpdateSet LimitedToMinor(params PackageInProject[] packages)
        {
            return LimitedToMinor(null, packages);
        }

        public static PackageUpdateSet LimitedToMinor(DateTimeOffset? publishedAt, params PackageInProject[] packages)
        {
            var latestId = new PackageIdentity("foo.bar", new NuGetVersion("2.3.4"));
            var latest = new PackageSearchMedatadata(latestId, OfficialPackageSource(), publishedAt, null);

            var match = new PackageSearchMedatadata(
                new PackageIdentity("foo.bar", new NuGetVersion("1.2.3")), OfficialPackageSource(), null, null);

            var updates = new PackageLookupResult(VersionChange.Minor, latest, match, null);
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
