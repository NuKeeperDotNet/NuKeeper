using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.NuGet.Api;

namespace NuKeeper.RepositoryInspection
{
    public class PackageUpdateSet
    {
        public PackageUpdateSet(VersionChange allowedChange,
            PackageSearchMedatadata highest,
            PackageSearchMedatadata match,
            IEnumerable<PackageInProject> currentPackages)
        {
            if (highest == null)
            {
                throw new ArgumentNullException(nameof(highest));
            }

            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            if (currentPackages == null)
            {
                throw new ArgumentNullException(nameof(currentPackages));
            }

            var currentPackagesList = currentPackages.ToList();

            if (!currentPackagesList.Any())
            {
                throw new ArgumentException($"{nameof(currentPackages)} is empty", nameof(currentPackages));
            }

            AllowedChange = allowedChange;
            Highest = highest;
            Match = match;
            CurrentPackages = currentPackagesList;

            CheckIdConsistency();
        }

        public VersionChange AllowedChange { get; }
        public PackageSearchMedatadata Highest { get; }
        public PackageSearchMedatadata Match { get; }

        public IReadOnlyCollection<PackageInProject> CurrentPackages { get; }

        public PackageIdentity NewPackage => Match.Identity;

        public string PackageId => Match.Identity.Id;
        public NuGetVersion NewVersion => Match.Identity.Version;

        public NuGetVersion HighestVersion=> Highest.Identity.Version;

        public int CountCurrentVersions()
        {
            return CurrentPackages
                .Select(p => p.Version)
                .Distinct()
                .Count();
        }

        private void CheckIdConsistency()
        {
            if (CurrentPackages.Any(p => p.Id != NewPackage.Id))
            {
                var errorIds = CurrentPackages
                    .Select(p => p.Id)
                    .Distinct()
                    .Where(id => id != NewPackage.Id);

                throw new ArgumentException($"Updates must all be for package '{NewPackage.Id}', got '{errorIds.JoinWithCommas()}'");
            }
        }
    }
}
