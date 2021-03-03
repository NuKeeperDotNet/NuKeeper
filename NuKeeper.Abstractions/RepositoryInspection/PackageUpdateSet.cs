using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Versioning;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.NuGetApi;

namespace NuKeeper.Abstractions.RepositoryInspection
{
    public class PackageUpdateSet
    {
        //TODO: should verify all currentPackages have a version lower than the Selected()
        // otherwise, it isn't an update...
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
        public VersionChange ActualChange => GetActualChange();
        public NuGetVersion HighestVersion => GetHighestVersion();

        public PackageSearchMetadata Selected => Packages.Selected();
        public PackageSearchMetadata Highest => GetHighest();

        public string SelectedId => Selected.Identity.Id;
        public NuGetVersion SelectedVersion => Selected.Identity.Version;
        public bool HigherVersionAvailable =>
            VersionComparer.Compare(HighestVersion, SelectedVersion, VersionComparison.Version) > 0;

        public int CountCurrentVersions()
        {
            return CurrentPackages
                .Select(p => p.Version)
                .Distinct()
                .Count();
        }

        private void CheckIdConsistency()
        {
            if (CurrentPackages.Any(p => !p.Id.Equals(SelectedId,StringComparison.InvariantCultureIgnoreCase)))
            {
                var errorIds = CurrentPackages
                    .Select(p => p.Id)
                    .Distinct()
                    .Where(id => !id.Equals(SelectedId,StringComparison.InvariantCultureIgnoreCase));

                throw new ArgumentException($"Updates must all be for package '{SelectedId}', got '{errorIds.JoinWithCommas()}'");
            }
        }

        private VersionChange GetActualChange()
        {
            var newVersion = SelectedVersion;
            var minVersion = CurrentPackages
                .Select(p => p.Version)
                .Min();

            if (newVersion.Major > minVersion.Major)
                return VersionChange.Major;
            else if (newVersion.Minor > minVersion.Minor)
                return VersionChange.Minor;
            else if (newVersion.Patch > minVersion.Patch)
                return VersionChange.Patch;
            else
                return VersionChange.None;
        }

        private PackageSearchMetadata GetHighest()
        {
            return Packages.Major ?? Packages.Minor ?? Packages.Patch;
        }

        private NuGetVersion GetHighestVersion()
        {
            return GetHighest().Identity.Version;
        }

        public override string ToString()
        {
            return $"{SelectedId} to {SelectedVersion} in {CurrentPackages.Count} places";
        }
    }
}
