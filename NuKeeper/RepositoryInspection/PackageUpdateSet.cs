using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Versioning;
using NuKeeper.NuGet.Api;

namespace NuKeeper.RepositoryInspection
{
    public class PackageUpdateSet
    {
        public PackageUpdateSet(PackageLookupResult packages, IEnumerable<PackageInProject> currentPackages)
        {
            if (packages == null)
            {
                throw new ArgumentNullException(nameof(packages));
            }

            if (packages.Major == null)
            {
                throw new ArgumentNullException(nameof(packages));
            }

            Packages = packages;

            if (currentPackages == null)
            {
                throw new ArgumentNullException(nameof(currentPackages));
            }

            var currentPackagesList = currentPackages.ToList();

            if (!currentPackagesList.Any())
            {
                throw new ArgumentException($"{nameof(currentPackages)} is empty", nameof(currentPackages));
            }

            CurrentPackages = currentPackagesList;
            CheckIdConsistency();
        }

        public PackageLookupResult Packages { get; }
        public IReadOnlyCollection<PackageInProject> CurrentPackages { get; }

        public VersionChange AllowedChange => Packages.AllowedChange;
        public PackageSearchMedatadata Highest => Packages.Major;
        public PackageSearchMedatadata Match => Packages.Selected();

        public string MatchId => Match.Identity.Id;
        public NuGetVersion MatchVersion => Match.Identity.Version;
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
            if (CurrentPackages.Any(p => p.Id != MatchId))
            {
                var errorIds = CurrentPackages
                    .Select(p => p.Id)
                    .Distinct()
                    .Where(id => id != MatchId);

                throw new ArgumentException($"Updates must all be for package '{MatchId}', got '{errorIds.JoinWithCommas()}'");
            }
        }
    }
}
