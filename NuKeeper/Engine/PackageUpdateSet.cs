using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.NuGet.Api;

namespace NuKeeper.Engine
{
    public class PackageUpdateSet
    {
        public PackageUpdateSet(PackageIdentity newPackage, IEnumerable<PackageUpdate> currentPackages)
        {
            if (newPackage == null)
            {
                throw new ArgumentNullException(nameof(newPackage));
            }

            if (currentPackages == null)
            {
                throw new ArgumentNullException(nameof(currentPackages));
            }

            NewPackage = newPackage;
            CurrentPackages = currentPackages.ToList();
        }


        public PackageIdentity NewPackage { get; }

        public List<PackageUpdate> CurrentPackages { get; }

        public string PackageId => NewPackage.Id;
        public NuGetVersion NewVersion => NewPackage.Version;
    }
}