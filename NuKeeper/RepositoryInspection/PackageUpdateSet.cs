using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace NuKeeper.RepositoryInspection
{
    public class PackageUpdateSet
    {
        public PackageUpdateSet(PackageIdentity newPackage, IEnumerable<PackageInProject> currentPackages)
        {
            if (newPackage == null)
            {
                throw new ArgumentNullException(nameof(newPackage));
            }

            if (currentPackages == null)
            {
                throw new ArgumentNullException(nameof(currentPackages));
            }

            var currentPackagesList = currentPackages.ToList();

            if (!currentPackagesList.Any())
            {
                throw new ArgumentException($"{nameof(currentPackages)} is empty");
            }

            if (currentPackagesList.Any(p => p.Id != newPackage.Id))
            {
                throw new ArgumentException($"Updates must all be for package {newPackage.Id}");
            }

            NewPackage = newPackage;
            CurrentPackages = currentPackagesList;
        }

        public PackageIdentity NewPackage { get; }

        public IReadOnlyCollection<PackageInProject> CurrentPackages { get; }

        public string PackageId => NewPackage.Id;
        public NuGetVersion NewVersion => NewPackage.Version;

        public int CountCurrentVersions()
        {
            return CurrentPackages
                .Select(p => p.Version)
                .Distinct()
                .Count();
        }
    }
}