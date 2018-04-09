using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Types.Logging;

namespace NuKeeper.Engine.Packages
{
    public class PackageUpdateSelection : IPackageUpdateSelection
    {
        private readonly INuKeeperLogger _logger;
        private readonly IExistingBranchFilter _existingBranchFilter;

        private readonly Regex _excludeFilter;
        private readonly Regex _includeFilter;
        private readonly int _maxPullRequests;
        private readonly DateTime _maxPublishedDate;

        public PackageUpdateSelection(UserSettings settings,
            INuKeeperLogger logger, IExistingBranchFilter existingBranchFilter)
        {
            _logger = logger;
            _existingBranchFilter = existingBranchFilter;

            _maxPullRequests = settings.MaxPullRequestsPerRepository;
            _includeFilter = settings.PackageIncludes;
            _excludeFilter = settings.PackageExcludes;
            _maxPublishedDate = DateTime.UtcNow.Subtract(settings.MinimumPackageAge);
        }

        public async Task<List<PackageUpdateSet>> SelectTargets(
            ForkData pushFork,
            IEnumerable<PackageUpdateSet> potentialUpdates)
        {
            var unfiltered = PackageUpdateSort.Sort(potentialUpdates)
                .ToList();

            var filtered = await ApplyFilters(pushFork, unfiltered);

            var capped = filtered
                .Take(_maxPullRequests)
                .ToList();

            LogPackageCounts(unfiltered.Count, filtered.Count, capped.Count);

            foreach (var updateSet in capped)
            {
                _logger.Info($"Selected package update of {updateSet.SelectedId} to {updateSet.SelectedVersion}");
            }

            return capped;
        }

        private async Task<List<PackageUpdateSet>> ApplyFilters(
            ForkData pushFork, IEnumerable<PackageUpdateSet> all)
        {
            var filteredLocally = all
                .Where(MatchesIncludeExclude)
                .Where(MatchesMinAge);

            var filtered = await _existingBranchFilter.CanMakeBranchFor(pushFork, filteredLocally);
            return filtered.ToList();
        }

        private void LogPackageCounts(int potential, int filtered, int capped)
        {
            var message = $"Selection of package updates: {potential} potential";
            if (filtered < potential)
            {
                message += $", filtered to {filtered}";
            }

            if (capped < filtered)
            {
                message +=  $", capped at {capped}";
            }

            _logger.Terse(message);
        }

        private bool MatchesIncludeExclude(PackageUpdateSet packageUpdateSet)
        {
            return 
                MatchesInclude(_includeFilter, packageUpdateSet)
                && ! MatchesExclude(_excludeFilter, packageUpdateSet);
        }

        private static bool MatchesInclude(Regex regex, PackageUpdateSet packageUpdateSet)
        {
            return regex == null || regex.IsMatch(packageUpdateSet.SelectedId);
        }

        private static bool MatchesExclude(Regex regex, PackageUpdateSet packageUpdateSet)
        {
            return regex != null && regex.IsMatch(packageUpdateSet.SelectedId);
        }

        private bool MatchesMinAge(PackageUpdateSet packageUpdateSet)
        {
            var published = packageUpdateSet.Selected.Published;
            if (!published.HasValue)
            {
                return true;
            }

            return published.Value.UtcDateTime <= _maxPublishedDate;
        }
    }
}
