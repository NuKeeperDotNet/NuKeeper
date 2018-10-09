using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Versioning;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.NuGetApi;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public class PackageUpdateSet
    {
        public PackageUpdateSet(PackageLookupResult packages, IEnumerable<PackageInProject> currentPackages)
        {
            if (packages == null)
            {
                throw new ArgumentNullException(nameof(packages));
            }

            if (packages.Selected() == null)
            {
                throw new ArgumentException("packages does not have a selected update", nameof(packages));
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
        public PackageSearchMedatadata Selected => Packages.Selected();

        public string SelectedId => Selected.Identity.Id;
        public NuGetVersion SelectedVersion => Selected.Identity.Version;

        public int CountCurrentVersions()
        {
            return CurrentPackages
                .Select(p => p.Version)
                .Distinct()
                .Count();
        }

        private void CheckIdConsistency()
        {
            if (CurrentPackages.Any(p => p.Id != SelectedId))
            {
                var errorIds = CurrentPackages
                    .Select(p => p.Id)
                    .Distinct()
                    .Where(id => id != SelectedId);

                throw new ArgumentException($"Updates must all be for package '{SelectedId}', got '{errorIds.JoinWithCommas()}'");
            }
        }

        public override string ToString()
        {
            return $"{SelectedId} to {SelectedVersion} in {CurrentPackages.Count} places";
        }
    }
}
