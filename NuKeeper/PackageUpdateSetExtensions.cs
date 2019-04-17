using System;
using System.Collections.Generic;
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
                packageLookupResult.Minor.ToAbstractions(),
                packageLookupResult.Patch.ToAbstractions());
        }

        private static IEnumerable<Abstractions.PackageInProject> ToAbstractions(this IEnumerable<NuKeeperInspection.RepositoryInspection.PackageInProject> packagesInProject)
        {
            return packagesInProject.Select(
                packageInProject => new NuKeeperAbstractions.PackageInProject(
                    packageInProject.PackageVersionRange,
                    packageInProject.Path.ToAbstractions(),
                    packageInProject.ProjectReferences)).ToList();
        }

        private static Abstractions.PackagePath ToAbstractions(this NuKeeperInspection.RepositoryInspection.PackagePath packagePath)
        {
            return new NuKeeperAbstractions.PackagePath(
                packagePath.BaseDirectory,
                packagePath.RelativePath,
                packagePath.PackageReferenceType.ToAbstractions());
        }

        private static Abstractions.PackageReferenceType ToAbstractions(this NuKeeperInspection.RepositoryInspection.PackageReferenceType packageReferenceType)
        {
            switch (packageReferenceType)
            {
                case NuKeeperInspection.RepositoryInspection.PackageReferenceType.DirectoryBuildTargets:
                    return NuKeeperAbstractions.PackageReferenceType.DirectoryBuildTargets;

                case NuKeeperInspection.RepositoryInspection.PackageReferenceType.PackagesConfig:
                    return NuKeeperAbstractions.PackageReferenceType.PackagesConfig;

                case NuKeeperInspection.RepositoryInspection.PackageReferenceType.ProjectFile:
                    return NuKeeperAbstractions.PackageReferenceType.ProjectFile;

                case NuKeeperInspection.RepositoryInspection.PackageReferenceType.ProjectFileOldStyle:
                    return NuKeeperAbstractions.PackageReferenceType.ProjectFileOldStyle;

                case NuKeeperInspection.RepositoryInspection.PackageReferenceType.Nuspec:
                    return NuKeeperAbstractions.PackageReferenceType.Nuspec;
            }

            throw new ArgumentException($"Could not map package reference type '{packageReferenceType}'.");
        }

        private static Abstractions.PackageSearchMedatadata ToAbstractions(this NuKeeperInspection.NuGetApi.PackageSearchMedatadata packageSearchMedatadata)
        {
            return new NuKeeperAbstractions.PackageSearchMedatadata(
                packageSearchMedatadata.Identity,
                packageSearchMedatadata.Source,
                packageSearchMedatadata.Published,
                packageSearchMedatadata.Dependencies);
        }
    }
}
