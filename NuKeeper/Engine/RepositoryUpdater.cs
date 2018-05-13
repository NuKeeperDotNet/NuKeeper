using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Engine.Packages;
using NuKeeper.Git;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Update.Process;

namespace NuKeeper.Engine
{
    public class RepositoryUpdater : IRepositoryUpdater
    {
        private readonly IUpdateFinder _updateFinder;
        private readonly IPackageUpdateSelection _updateSelection;
        private readonly IPackageUpdater _packageUpdater;
        private readonly INuKeeperLogger _logger;
        private readonly SolutionsRestore _solutionsRestore;
        private readonly IAvailableUpdatesReporter _availableUpdatesReporter;
        private readonly UserSettings _settings;

        public RepositoryUpdater(
            IUpdateFinder updateFinder,
            IPackageUpdateSelection updateSelection,
            IPackageUpdater packageUpdater,
            INuKeeperLogger logger,
            SolutionsRestore solutionsRestore,
            IAvailableUpdatesReporter availableUpdatesReporter,
            UserSettings settings)
        {
            _updateFinder = updateFinder;
            _updateSelection = updateSelection;
            _packageUpdater = packageUpdater;
            _logger = logger;
            _solutionsRestore = solutionsRestore;
            _availableUpdatesReporter = availableUpdatesReporter;
            _settings = settings;
        }

        public async Task Run(IGitDriver git, RepositoryData repository)
        {
            GitInit(git, repository);

            var updates = await _updateFinder.FindPackageUpdateSets(git.WorkingFolder);

            _logger.Verbose($"Report mode is {_settings.ReportMode}");
            switch (_settings.ReportMode)
            {
                case ReportMode.Off:
                    break;

                case ReportMode.On:
                    // report and continue
                    _availableUpdatesReporter.Report(repository.Pull.Name, updates);
                    break;

                case ReportMode.ReportOnly:
                    // report and exit
                    _availableUpdatesReporter.Report(repository.Pull.Name, updates);
                    _logger.Info("Exiting after reports only");
                    return;

                default:
                    throw new Exception($"Unknown report mode: '{_settings.ReportMode}'");
            }

            if (updates.Count == 0)
            {
                _logger.Terse("No potential updates found. Well done. Exiting.");
                return;
            }

            var targetUpdates = await _updateSelection.SelectTargets(repository.Push, updates);

            if (updates.Count == 0)
            {
                _logger.Terse("No updates can be applied. Exiting.");
                return;
            }

            await _solutionsRestore.Restore(git.WorkingFolder);

            await UpdateAllTargets(git, repository, targetUpdates);

            _logger.Info($"Done {targetUpdates.Count} Updates");
        }

        private static void GitInit(IGitDriver git, RepositoryData repository)
        {
            git.Clone(repository.Pull.Uri);
            repository.DefaultBranch = git.GetCurrentHead();
            git.AddRemote("nukeeper_push", repository.Push.Uri);
        }

        private async Task UpdateAllTargets(IGitDriver git,
            RepositoryData repository,
            IEnumerable<PackageUpdateSet> targetUpdates)
        {
            foreach (var updateSet in targetUpdates)
            {
                await _packageUpdater.MakeUpdatePullRequest(git, updateSet, repository);
            }
        }
    }
}
