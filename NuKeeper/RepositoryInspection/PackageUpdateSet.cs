using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace NuKeeper.RepositoryInspection
{
    public class PackageUpdateSet
    {
        public PackageUpdateSet(PackageIdentity newPackage, string packageSource, IEnumerable<PackageInProject> currentPackages)
        {
            if (newPackage == null)
            {
                throw new ArgumentNullException(nameof(newPackage));
            }

            if (currentPackages == null)
            {
                throw new ArgumentNullException(nameof(currentPackages));
            }

            if (packageSource == null)
            {
                throw new ArgumentNullException(nameof(packageSource));
            }

            var currentPackagesList = currentPackages.ToList();

            if (!currentPackagesList.Any())
            {
                throw new ArgumentException($"{nameof(currentPackages)} is empty", nameof(currentPackages));
            }

            if (currentPackagesList.Any(p => p.Id != newPackage.Id))
            {
                var errorIds = currentPackagesList
                    .Select(p => p.Id)
                    .Distinct()
                    .Where(id => id != newPackage.Id);

                throw new ArgumentException($"Updates must all be for package '{newPackage.Id}', got '{errorIds.JoinWithCommas()}'");
            }

            NewPackage = newPackage;
            CurrentPackages = currentPackagesList;
            PackageSource = packageSource;
        }

        public PackageIdentity NewPackage { get; }

        public IReadOnlyCollection<PackageInProject> CurrentPackages { get; }

        public string PackageId => NewPackage.Id;
        public NuGetVersion NewVersion => NewPackage.Version;
        public string PackageSource { get; }

        public int CountCurrentVersions()
        {
            return CurrentPackages
                .Select(p => p.Version)
                .Distinct()
                .Count();
        }

        public bool UsesPackagesConfigFile()
        {
            return CurrentPackages.Any(
                p => p.Path.PackageReferenceType == PackageReferenceType.PackagesConfig);
        }
    }
}