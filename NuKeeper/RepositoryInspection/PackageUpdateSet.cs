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
        private readonly PackageSearchMedatadata _match;
        private readonly PackageSearchMedatadata _highest;

        public PackageUpdateSet(
            PackageSearchMedatadata match,
            PackageSearchMedatadata highest,
            VersionChange allowedChange,
            IEnumerable<PackageInProject> currentPackages)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            if (highest == null)
            {
                throw new ArgumentNullException(nameof(highest));
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

            _match = match;
            _highest = highest;
            AllowedChange = allowedChange;
            CurrentPackages = currentPackagesList;

            CheckIdConsistency();
        }

        public VersionChange AllowedChange { get; }
        public IReadOnlyCollection<PackageInProject> CurrentPackages { get; }

        public PackageIdentity NewPackage => _match.Identity;

        public string PackageId => NewPackage.Id;
        public NuGetVersion NewVersion => NewPackage.Version;
        public string PackageSource => _match.Source;
        public NuGetVersion Highest=> _highest.Identity.Version;
        public DateTimeOffset? HighestPublished => _highest.Published;

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
