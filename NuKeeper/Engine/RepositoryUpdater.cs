using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Engine.Packages;
using NuKeeper.Git;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update.Process;

namespace NuKeeper.Engine
{
    public class RepositoryUpdater : IRepositoryUpdater
    {
        private readonly INuGetSourcesReader _nugetSourcesReader;
        private readonly IUpdateFinder _updateFinder;
        private readonly IPackageUpdateSelection _updateSelection;
        private readonly IPackageUpdater _packageUpdater;
        private readonly INuKeeperLogger _logger;
        private readonly SolutionsRestore _solutionsRestore;
        private readonly IAvailableUpdatesReporter _availableUpdatesReporter;

        public RepositoryUpdater(
            INuGetSourcesReader nugetSourcesReader,
            IUpdateFinder updateFinder,
            IPackageUpdateSelection updateSelection,
            IPackageUpdater packageUpdater,
            INuKeeperLogger logger,
            SolutionsRestore solutionsRestore,
            IAvailableUpdatesReporter availableUpdatesReporter)
        {
            _nugetSourcesReader = nugetSourcesReader;
            _updateFinder = updateFinder;
            _updateSelection = updateSelection;
            _packageUpdater = packageUpdater;
            _logger = logger;
            _solutionsRestore = solutionsRestore;
            _availableUpdatesReporter = availableUpdatesReporter;
        }

        public async Task<int> Run(
            IGitDriver git,
            RepositoryData repository,
            SettingsContainer settings)
        {
            GitInit(git, repository);

            var sources = _nugetSourcesReader.Read(git.WorkingFolder, settings.UserSettings.NuGetSources);

            var updates = await _updateFinder.FindPackageUpdateSets(
                git.WorkingFolder, sources, settings.UserSettings.AllowedChange);

            _logger.Detailed($"Report mode is {settings.UserSettings.ReportMode}");
            switch (settings.UserSettings.ReportMode)
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
                    _logger.Normal("Exiting after reports only");
                    return 0;

                default:
                    throw new Exception($"Unknown report mode: '{settings.UserSettings.ReportMode}'");
            }

            if (updates.Count == 0)
            {
                _logger.Minimal("No potential updates found. Well done. Exiting.");
                return 0;
            }

            var targetUpdates = await _updateSelection.SelectTargets(
                repository.Push, updates, settings.PackageFilters);

            return await DoTargetUpdates(git, repository, targetUpdates, sources);
        }

        private async Task<int> DoTargetUpdates(
            IGitDriver git, RepositoryData repository,
            IReadOnlyCollection<PackageUpdateSet> targetUpdates,
            NuGetSources sources)
        {
            if (targetUpdates.Count == 0)
            {
                _logger.Minimal("No updates can be applied. Exiting.");
                return 0;
            }

            if (AnyProjectRequiresNuGetRestore(targetUpdates))
            {
                await _solutionsRestore.Restore(git.WorkingFolder, sources);
            }

            var updatesDone = await UpdateAllTargets(git, repository, targetUpdates, sources);

            if (updatesDone < targetUpdates.Count)
            {
                _logger.Minimal($"Attempted {targetUpdates.Count} updates and did {updatesDone}");
            }
            else
            {
                _logger.Normal($"Done {updatesDone} updates");
            }

            return updatesDone;
        }

        private static bool AnyProjectRequiresNuGetRestore(IEnumerable<PackageUpdateSet> targetUpdates)
        {
            return targetUpdates.SelectMany(u => u.CurrentPackages)
                .Any(p => p.Path.PackageReferenceType != PackageReferenceType.ProjectFile);
        }

        private static void GitInit(IGitDriver git, RepositoryData repository)
        {
            git.Clone(repository.Pull.Uri);
            repository.DefaultBranch = git.GetCurrentHead();
            git.AddRemote("nukeeper_push", repository.Push.Uri);
        }

        private async Task<int> UpdateAllTargets(IGitDriver git,
            RepositoryData repository,
            IEnumerable<PackageUpdateSet> targetUpdates,
            NuGetSources sources)
        {
            var updatesDone = 0;

            foreach (var updateSet in targetUpdates)
            {
                var success = await _packageUpdater.MakeUpdatePullRequest(git, updateSet, sources, repository);
                if (success)
                {
                    updatesDone++;
                }
            }

            return updatesDone;
        }
    }
}
