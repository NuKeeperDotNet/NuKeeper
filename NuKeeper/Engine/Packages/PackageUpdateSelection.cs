using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NuKeeper.Configuration;
using NuKeeper.Git;
using NuKeeper.Logging;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine.Packages
{
    public class PackageUpdateSelection : IPackageUpdateSelection
    {
        private readonly INuKeeperLogger _logger;
        private readonly Regex _excludeFilter;
        private readonly Regex _includeFilter;
        private readonly int _maxPullRequests;

        public PackageUpdateSelection(UserSettings settings, INuKeeperLogger logger)
        {
            _logger = logger;
            _maxPullRequests = settings.MaxPullRequestsPerRepository;
            _includeFilter = settings.PackageIncludes;
            _excludeFilter = settings.PackageExcludes;
        }

        public List<PackageUpdateSet> SelectTargets(
            IGitDriver git,
            IEnumerable<PackageUpdateSet> potentialUpdates)
        {
            var unfiltered = PackageUpdateSort.Sort(potentialUpdates)
                .ToList();

            var filtered = unfiltered
                .Where(MatchesIncludeExclude)
                .Where(up => !HasExistingBranch(git, up))
                .ToList();

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

        private static bool HasExistingBranch(IGitDriver git, PackageUpdateSet packageUpdateSet)
        {
            var qualifiedBranchName = "origin/" + BranchNamer.MakeName(packageUpdateSet);
            return git.BranchExists(qualifiedBranchName);
        }
    }
}
