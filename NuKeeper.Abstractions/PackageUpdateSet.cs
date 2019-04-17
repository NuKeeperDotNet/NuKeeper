using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Versioning;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Abstractions
{
    public class PackageUpdateSet
    {
        public PackageUpdateSet(PackageLookupResult packages, IEnumerable<PackageInProject> currentPackages)
        {
            Packages = packages ?? throw new ArgumentNullException(nameof(packages));
            CurrentPackages = currentPackages?.ToList() ?? throw new ArgumentNullException(nameof(currentPackages));
        }

        public PackageLookupResult Packages { get; }

        public IReadOnlyCollection<PackageInProject> CurrentPackages { get; }

        public VersionChange AllowedChange => Packages.AllowedChange;

        public PackageSearchMedatadata Selected => Packages.Selected;

        public string SelectedId => Selected.Identity.Id;

        public NuGetVersion SelectedVersion => Selected.Identity.Version;
    }
}
