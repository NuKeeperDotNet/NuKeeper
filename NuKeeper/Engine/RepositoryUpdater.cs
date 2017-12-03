using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Git;
using NuKeeper.Logging;
using NuKeeper.NuGet.Api;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public class RepositoryUpdater : IRepositoryUpdater
    {   
        private readonly IPackageUpdatesLookup _packageLookup;
        private readonly IPackageUpdateSelection _updateSelection;
        private readonly IPackageUpdater _packageUpdater;
        private readonly IRepositoryScanner _repositoryScanner;
        private readonly INuKeeperLogger _logger;
        private readonly SolutionsRestore _solutionsRestore;

        public RepositoryUpdater(
            IPackageUpdatesLookup packageLookup, 
            IPackageUpdateSelection updateSelection,
            IPackageUpdater packageUpdater,
            IRepositoryScanner repositoryScanner,
            INuKeeperLogger logger,
            SolutionsRestore solutionsRestore)
        {
            _packageLookup = packageLookup;
            _updateSelection = updateSelection;
            _packageUpdater = packageUpdater;
            _repositoryScanner = repositoryScanner;
            _logger = logger;
            _solutionsRestore = solutionsRestore;
        }

        public async Task Run(IGitDriver git, RepositorySpec repository)
        {
            GitInit(git, repository);

            var updates = await FindPackageUpdateSets(git);

            if (updates.Count == 0)
            {
                _logger.Terse("No potential updates found. Well done. Exiting.");
                return;
            }

            var targetUpdates = _updateSelection.SelectTargets(git, updates);

            if (updates.Count == 0)
            {
                _logger.Terse("No updates can be applied. Exiting.");
                return;
            }

            await _solutionsRestore.Restore(git.WorkingFolder);

            await UpdateAllTargets(git, repository, targetUpdates);

            _logger.Info($"Done {targetUpdates.Count} Updates");
        }

        private static void GitInit(IGitDriver git, RepositorySpec repository)
        {
            git.Clone(repository.Pull.Uri);
            repository.DefaultBranch = git.GetCurrentHead();
            git.AddRemote("nukeeper_push", repository.Push.Uri);
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

        private async Task UpdateAllTargets(IGitDriver git,
            RepositorySpec repository,
            IEnumerable<PackageUpdateSet> targetUpdates)
        {
            foreach (var updateSet in targetUpdates)
            {
                await _packageUpdater.UpdatePackageInProjects(git, updateSet, repository);
            }
        }
    }
}
