using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NuKeeper.Configuration;
using NuKeeper.Git;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public class PackageUpdateSelection : IPackageUpdateSelection
    {
        private readonly Regex _excludeFilter;
        private readonly Regex _includeFilter;
        private readonly int _maxPullRequests;

        public PackageUpdateSelection(Settings settings)
        {
            _maxPullRequests = settings.MaxPullRequestsPerRepository;
            _includeFilter = settings.PackageIncludes;
            _excludeFilter = settings.PackageExcludes;
        }

        public List<PackageUpdateSet> SelectTargets(
            IGitDriver git,
            IEnumerable<PackageUpdateSet> potentialUpdates)
        {
            return potentialUpdates
                .OrderByDescending(Priority)
                .Where(MatchesIncludeExclude)
                .Where(up => ! HasExistingBranch(git, up))
                .Take(_maxPullRequests)
                .ToList();
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
