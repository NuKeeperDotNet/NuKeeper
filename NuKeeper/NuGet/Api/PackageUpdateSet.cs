using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Nuget.Api
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

            if (!currentPackages.Any())
            {
                throw new ArgumentException($"{nameof(currentPackages)} is empty");
            }

            if (currentPackages.Any(p => p.Id != newPackage.Id))
            {
                throw new ArgumentException($"Updates must all be for package {newPackage.Id}");
            }

            NewPackage = newPackage;
            CurrentPackages = currentPackages.ToList();
        }

        public PackageIdentity NewPackage { get; }

        public List<PackageInProject> CurrentPackages { get; }

        public string PackageId => NewPackage.Id;
        public NuGetVersion NewVersion => NewPackage.Version;
    }
}