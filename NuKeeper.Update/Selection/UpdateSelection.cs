using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using System.Threading.Tasks;

namespace NuKeeper.Update.Selection
{
    public class UpdateSelection : IUpdateSelection
    {
        private readonly INuKeeperLogger _logger;

        private readonly FilterSettings _settings;
        private readonly DateTime _maxPublishedDate;

        public UpdateSelection(FilterSettings settings,
            INuKeeperLogger logger)
        {
            _logger = logger;
            _settings = settings;
            _maxPublishedDate = DateTime.UtcNow.Subtract(settings.MinimumAge);
        }

        public async Task<IReadOnlyCollection<PackageUpdateSet>> Filter(
            IReadOnlyCollection<PackageUpdateSet> candidates,
            Func<PackageUpdateSet, Task<bool>> remoteCheck)
        {
            var filtered = await ApplyFilters(candidates, remoteCheck);

            List<PackageUpdateSet> capped = filtered
                .Take(_settings.MaxPullRequests)
                .ToList();

            LogPackageCounts(candidates.Count, filtered.Count, capped.Count);

            foreach (var updateSet in capped)
            {
                _logger.Info($"Selected package update of {updateSet.SelectedId} to {updateSet.SelectedVersion}");
            }

            return capped;
        }

        private async Task<IReadOnlyCollection<PackageUpdateSet>> ApplyFilters(
            IReadOnlyCollection<PackageUpdateSet> all,
            Func<PackageUpdateSet, Task<bool>> remoteCheck)
        {
            var filteredLocally = all
                .Where(MatchesIncludeExclude)
                .Where(MatchesMinAge)
                .ToList();

            if (filteredLocally.Count < all.Count)
            {
                _logger.Verbose($"Filtered by rules from {all.Count} to {filteredLocally.Count}");
            }

            var remoteFiltered = await ApplyRemoteFilter(filteredLocally, remoteCheck);

            if (remoteFiltered.Count < filteredLocally.Count)
            {
                _logger.Verbose($"Filtered by remote branch check branch from {filteredLocally.Count} to {remoteFiltered.Count}");
            }

            return remoteFiltered;
        }

        public static async Task<IReadOnlyCollection<PackageUpdateSet>> ApplyRemoteFilter(
            IEnumerable<PackageUpdateSet> packageUpdateSets,
            Func<PackageUpdateSet, Task<bool>> remoteCheck)
        {
            var results = await packageUpdateSets
                .WhereAsync(async p => await remoteCheck(p));

            return results.ToList();
        }

        private void LogPackageCounts(int candidates, int filtered, int capped)
        {
            var message = $"Selection of package updates: {candidates} candiates";
            if (filtered < candidates)
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
                MatchesInclude(_settings.Includes, packageUpdateSet)
                && ! MatchesExclude(_settings.Excludes, packageUpdateSet);
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
