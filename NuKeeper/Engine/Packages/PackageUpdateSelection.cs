using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Engine.Sort;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;

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

        public async Task<IReadOnlyCollection<PackageUpdateSet>> SelectTargets(
            ForkData pushFork,
            IEnumerable<PackageUpdateSet> potentialUpdates)
        {
            var unfiltered = PackageUpdateSort.Sort(potentialUpdates, _logger)
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

        private async Task<IReadOnlyCollection<PackageUpdateSet>> ApplyFilters(
            ForkData pushFork, IReadOnlyCollection<PackageUpdateSet> all)
        {
            var filteredLocally = all
                .Where(MatchesIncludeExclude)
                .Where(MatchesMinAge)
                .ToList();

            if (filteredLocally.Count < all.Count)
            {
                _logger.Verbose($"Filtered by rules from {all.Count} to {filteredLocally.Count}");
            }

            var filteredByBranch = await _existingBranchFilter.CanMakeBranchFor(pushFork, filteredLocally);

            if (filteredByBranch.Count < filteredLocally.Count)
            {
                _logger.Verbose($"Filtered by existing branch from {filteredLocally.Count} to {filteredByBranch.Count}");
            }

            return filteredByBranch;
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
