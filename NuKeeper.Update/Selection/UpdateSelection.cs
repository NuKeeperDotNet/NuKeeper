using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.RepositoryInspection;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public IReadOnlyCollection<PackageUpdateSet> Filter(
            IReadOnlyCollection<PackageUpdateSet> candidates,
            FilterSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            if (settings.MinimumAge != TimeSpan.Zero)
            {
                _maxPublishedDate = DateTime.UtcNow.Subtract(settings.MinimumAge);
            }

            var filtered = ApplyFilters(candidates);

            var capped = filtered
                .Take(settings.MaxPackageUpdates)
                .ToList();

            LogPackageCounts(candidates.Count, filtered.Count, capped.Count);

            return capped;
        }

        private IReadOnlyCollection<PackageUpdateSet> ApplyFilters(
            IReadOnlyCollection<PackageUpdateSet> all)
        {
            var filtered = all
                .Where(MatchesMinAge)
                .ToList();

            if (filtered.Count < all.Count)
            {
                var agoFormat = TimeSpanFormat.Ago(_settings.MinimumAge);
                _logger.Normal($"Filtered by minimum package age '{agoFormat}' from {all.Count} to {filtered.Count}");
            }

            return filtered;
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
                message += $", capped at {capped}";
            }

            _logger.Minimal(message);
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
