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
        public static PackageUpdateSet MakePackageUpdateSet(string packageName)
        {
            var fooPackage = new PackageIdentity(packageName, new NuGetVersion("1.2.3"));
            var latest = new PackageSearchMedatadata(fooPackage, new PackageSource("http://none"), null,
                Enumerable.Empty<PackageDependency>());
            var packages = new PackageLookupResult(VersionChange.Major, latest, null, null);

            var path = new PackagePath("c:\\foo", "bar", PackageReferenceType.ProjectFile);
            var pip = new PackageInProject(fooPackage, path, null);

            return new PackageUpdateSet(packages, new List<PackageInProject> { pip });
        }

    }
}
