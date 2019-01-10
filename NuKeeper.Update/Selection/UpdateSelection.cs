using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NuKeeper.Inspection.RepositoryInspection;
using System.Threading.Tasks;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.Update.Selection
{
    public class UpdateSelection : IUpdateSelection
    {
        private readonly INuKeeperLogger _logger;
        private FilterSettings _settings;
        private DateTime? _maxPublishedDate = null;

        public UpdateSelection(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<PackageUpdateSet>> Filter(
            IReadOnlyCollection<PackageUpdateSet> candidates,
            FilterSettings settings,
            Func<PackageUpdateSet, Task<bool>> remoteCheck)
        {
            _settings = settings;
            if (settings.MinimumAge != TimeSpan.Zero)
            {
                _maxPublishedDate = DateTime.UtcNow.Subtract(settings.MinimumAge);
            }

            var filtered = await ApplyFilters(candidates, remoteCheck);

            var capped = filtered
                .ToList();

            if (settings.MaxPackageUpdates > 0)
            {
                capped = capped.Take(settings.MaxPackageUpdates).ToList();
            }

            LogPackageCounts(candidates.Count, filtered.Count, capped.Count);

            return capped;
        }

        private async Task<IReadOnlyCollection<PackageUpdateSet>> ApplyFilters(
            IReadOnlyCollection<PackageUpdateSet> all,
            Func<PackageUpdateSet, Task<bool>> remoteCheck)
        {
            var filteredByInOut = FilteredByIncludeExclude(all);

            var filteredLocally = filteredByInOut
                .Where(MatchesMinAge)
                .ToList();

            if (filteredLocally.Count < filteredByInOut.Count)
            {
                var agoFormat = TimeSpanFormat.Ago(_settings.MinimumAge);
                _logger.Normal($"Filtered by minimum package age '{agoFormat}' from {filteredByInOut.Count} to {filteredLocally.Count}");
            }

            var remoteFiltered = await ApplyRemoteFilter(filteredLocally, remoteCheck);

            if (remoteFiltered.Count < filteredLocally.Count)
            {
                _logger.Normal($"Filtered by remote branch check branch from {filteredLocally.Count} to {remoteFiltered.Count}");
            }

            return remoteFiltered;
        }

        private IReadOnlyCollection<PackageUpdateSet> FilteredByIncludeExclude(IReadOnlyCollection<PackageUpdateSet> all)
        {
            var filteredByIncludeExclude = all
                .Where(MatchesIncludeExclude)
                .ToList();

            if (filteredByIncludeExclude.Count < all.Count)
            {
                var filterDesc = string.Empty;
                if (_settings.Excludes != null)
                {
                    filterDesc += $"Exclude '{_settings.Excludes}'";
                }

                if (_settings.Includes != null)
                {
                    filterDesc += $"Include '{_settings.Includes}'";
                }

                _logger.Normal($"Filtered by {filterDesc} from {all.Count} to {filteredByIncludeExclude.Count}");
            }

            return filteredByIncludeExclude;
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
            var message = $"Selection of package updates: {candidates} candidates";
            if (filtered < candidates)
            {
                message += $", filtered to {filtered}";
            }

            if (capped < filtered)
            {
                message +=  $", capped at {capped}";
            }

            _logger.Minimal(message);
        }

        private bool MatchesIncludeExclude(PackageUpdateSet packageUpdateSet)
        {
            return RegexMatch.IncludeExclude(packageUpdateSet.SelectedId,
                _settings.Includes, _settings.Excludes);
        }

        private bool MatchesMinAge(PackageUpdateSet packageUpdateSet)
        {
            if (!_maxPublishedDate.HasValue)
            {
                return true;
            }

            var published = packageUpdateSet.Selected.Published;
            if (!published.HasValue)
            {
                return true;
            }

            return published.Value.UtcDateTime <= _maxPublishedDate.Value;
        }
    }
}
