using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Git;
using NuKeeper.Logging;
using NuKeeper.NuGet.Api;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public class RepositoryUpdater : IRepositoryUpdater
    {   
        private readonly IPackageUpdatesLookup _packageLookup;
        private readonly IExistingBranchFilter _existingBranchFilter;
        private readonly IPackageUpdateSelection _updateSelection;
        private readonly IPackageUpdater _packageUpdater;
        private readonly IRepositoryScanner _repositoryScanner;
        private readonly INuKeeperLogger _logger;

        public RepositoryUpdater(
            IPackageUpdatesLookup packageLookup,
            IExistingBranchFilter existingBranchFilter,
            IPackageUpdateSelection updateSelection,
            IPackageUpdater packageUpdater,
            IRepositoryScanner repositoryScanner,
            INuKeeperLogger logger)
        {
            _packageLookup = packageLookup;
            _existingBranchFilter = existingBranchFilter;
            _updateSelection = updateSelection;
            _packageUpdater = packageUpdater;
            _repositoryScanner = repositoryScanner;
            _logger = logger;
        }

        public async Task Run(IGitDriver git, RepositoryModeSettings settings)
        {
            git.Clone(settings.GithubUri);
            var defaultBranch = git.GetCurrentHead();

            var updates = await FindPackageUpdateSets(git);

            if (updates.Count == 0)
            {
                _logger.Terse("No potential updates found. Well done. Exiting.");
                return;
            }

            var targetUpdates = SelectTargetUpdates(git, updates);

            await UpdateAllTargets(git, settings, targetUpdates, defaultBranch);

            _logger.Info($"Done {targetUpdates.Count} Updates");
        }

        private async Task<List<PackageUpdateSet>> FindPackageUpdateSets(IGitDriver git)
        {
            // scan for nuget packages
            var packages = _repositoryScanner.FindAllNuGetPackages(git.WorkingFolder)
                .ToList();

            _logger.Log(EngineReport.PackagesFound(packages));

            // look for package updates
            var updates = await _packageLookup.FindUpdatesForPackages(packages);
            _logger.Log(EngineReport.UpdatesFound(updates));
            return updates;
        }

        private List<PackageUpdateSet> SelectTargetUpdates(IGitDriver git, List<PackageUpdateSet> updates)
        {
            var noExistingBranch = updates
                .Where(u => !_existingBranchFilter.Exists(git, u));
            return _updateSelection.SelectTargets(noExistingBranch);
        }

        private async Task UpdateAllTargets(IGitDriver git, RepositoryModeSettings settings, IEnumerable<PackageUpdateSet> targetUpdates, string defaultBranch)
        {
            foreach (var updateSet in targetUpdates)
            {
                await _packageUpdater.UpdatePackageInProjects(git, updateSet, settings, defaultBranch);
            }
        }
    }
}
