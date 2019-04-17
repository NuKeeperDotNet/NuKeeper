using System.Collections.Generic;
using System.IO;
using System.Linq;

using NuKeeperAbstractions = NuKeeper.Abstractions;
using NuKeeperInspection = NuKeeper.Inspection;

namespace NuKeeper
{
    internal static class PackageUpdateSetExtensions
    {
        internal static IReadOnlyCollection<Abstractions.PackageUpdateSet> ToAbstractions(this IReadOnlyCollection<NuKeeperInspection.RepositoryInspection.PackageUpdateSet> packageUpdateSets)
        {
            return packageUpdateSets.Select(packageUpdateSet => packageUpdateSet.ToAbstractions()).ToList();
        }

        internal static Abstractions.PackageUpdateSet ToAbstractions(this NuKeeperInspection.RepositoryInspection.PackageUpdateSet packageUpdateSet)
        {
            return new NuKeeperAbstractions.PackageUpdateSet(
                packageUpdateSet.Packages.ToAbstractions(),
                packageUpdateSet.CurrentPackages.ToAbstractions());
        }

        private static Abstractions.PackageLookupResult ToAbstractions(this NuKeeperInspection.NuGetApi.PackageLookupResult packageLookupResult)
        {
            return new NuKeeperAbstractions.PackageLookupResult(
                packageLookupResult.AllowedChange,
                packageLookupResult.Major.ToAbstractions(),
                packageLookupResult.Selected().ToAbstractions());
        }

        private static IEnumerable<Abstractions.PackageInProject> ToAbstractions(this IEnumerable<NuKeeperInspection.RepositoryInspection.PackageInProject> packagesInProject)
        {
            return packagesInProject.Select(
                packageInProject => new NuKeeperAbstractions.PackageInProject(
                    packageInProject.PackageVersionRange,
                    packageInProject.Path.ToAbstractions())).ToList();
        }

        private static Abstractions.PackagePath ToAbstractions(this NuKeeperInspection.RepositoryInspection.PackagePath packagePath)
        {
            var fullPath = Path.Combine(packagePath.BaseDirectory, packagePath.RelativePath);
            var fullName = new FileInfo(fullPath).FullName;

            return new NuKeeperAbstractions.PackagePath(
                packagePath.RelativePath,
                fullName);
        }

        private static Abstractions.PackageSearchMedatadata ToAbstractions(this NuKeeperInspection.NuGetApi.PackageSearchMedatadata packageSearchMedatadata)
        {
            return new NuKeeperAbstractions.PackageSearchMedatadata(
                packageSearchMedatadata.Identity,
                packageSearchMedatadata.Source.SourceUri,
                packageSearchMedatadata.Published);
        }
    }
}
