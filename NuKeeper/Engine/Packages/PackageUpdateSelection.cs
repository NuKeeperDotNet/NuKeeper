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
            var unfiltered = potentialUpdates
                .OrderByDescending(Priority)
                .ToList();

            var filtered = unfiltered
                .Where(MatchesIncludeExclude)
                .Where(up => !HasExistingBranch(git, up))
                .ToList();

            var capped = filtered
                .Take(_maxPullRequests)
                .ToList();

            _logger.Terse($"Selection of package updates: {unfiltered.Count} potential, filtered to {filtered.Count} and capped at {capped.Count}");

            foreach (var updateSet in capped)
            {
                _logger.Info($"Selected package update of {updateSet.PackageId} to {updateSet.NewVersion}");
            }

            return capped;
        }

        private int Priority(PackageUpdateSet update)
        {
            return update.CountCurrentVersions();
        }

        private bool MatchesIncludeExclude(PackageUpdateSet packageUpdateSet)
        {
            return 
                MatchesInclude(_includeFilter, packageUpdateSet)
                && ! MatchesExclude(_excludeFilter, packageUpdateSet);
        }

        private static bool MatchesInclude(Regex regex, PackageUpdateSet packageUpdateSet)
        {
            return regex == null || regex.IsMatch(packageUpdateSet.PackageId);
        }

        private static bool MatchesExclude(Regex regex, PackageUpdateSet packageUpdateSet)
        {
            return regex != null && regex.IsMatch(packageUpdateSet.PackageId);
        }

        private static bool HasExistingBranch(IGitDriver git, PackageUpdateSet packageUpdateSet)
        {
            var qualifiedBranchName = "origin/" + BranchNamer.MakeName(packageUpdateSet);
            return git.BranchExists(qualifiedBranchName);
        }
    }
}
