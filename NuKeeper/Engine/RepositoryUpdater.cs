using System;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Engine.Packages;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update.Process;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.ObjectModel;

namespace NuKeeper.Engine
{
    public class RepositoryUpdater : IRepositoryUpdater
    {
        private readonly INuGetSourcesReader _nugetSourcesReader;
        private readonly IUpdateFinder _updateFinder;
        private readonly IPackageUpdateSelection _updateSelection;
        private readonly IPackageUpdater _packageUpdater;
        private readonly INuKeeperLogger _logger;
        private readonly ISolutionRestore _solutionRestore;
        private readonly IReporter _reporter;

        public RepositoryUpdater(
            INuGetSourcesReader nugetSourcesReader,
            IUpdateFinder updateFinder,
            IPackageUpdateSelection updateSelection,
            IPackageUpdater packageUpdater,
            INuKeeperLogger logger,
            ISolutionRestore solutionRestore,
            IReporter reporter
        )
        {
            _nugetSourcesReader = nugetSourcesReader;
            _updateFinder = updateFinder;
            _updateSelection = updateSelection;
            _packageUpdater = packageUpdater;
            _logger = logger;
            _solutionRestore = solutionRestore;
            _reporter = reporter;
        }

        public async Task<int> Run(
            IGitDriver git,
            RepositoryData repository,
            SettingsContainer settings)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (git == null)
            {
                throw new ArgumentNullException(nameof(git));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (!repository.IsLocalRepo)
            {
                await GitInit(git, repository);
            }

            var userSettings = settings.UserSettings;

            var sources = _nugetSourcesReader.Read(settings.WorkingFolder ?? git.WorkingFolder, userSettings.NuGetSources);

            var updates = await _updateFinder.FindPackageUpdateSets(
                settings.WorkingFolder ?? git.WorkingFolder,
                sources,
                userSettings.AllowedChange,
                userSettings.UsePrerelease,
                settings.PackageFilters?.Includes,
                settings.PackageFilters?.Excludes);

            _reporter.Report(
                userSettings.OutputDestination,
                userSettings.OutputFormat,
                repository.Pull.Name,
                userSettings.OutputFileName,
                updates);

            if (updates.Count == 0)
            {
                _logger.Minimal("No potential updates found. Well done. Exiting.");
                return 0;
            }

            while (updates.Any())
            {
                var targetUpdates = _updateSelection.SelectTargets(
                    repository.Push,
                    updates,
                    settings.PackageFilters
                );

                if (!targetUpdates.Any())
                {
                    _logger.Minimal("No updates can be applied. Exiting.");
                    return 0;
                }

                var (updatesDone, thresholdReached) = await DoTargetUpdates(git, repository, targetUpdates,
                    sources, settings);

                if (updatesDone != 0)
                {
                    return updatesDone;
                }

                if (thresholdReached.GetValueOrDefault())
                    return 0;

                updates = new ReadOnlyCollection<PackageUpdateSet>(
                    updates.Except(targetUpdates).ToList()
                );
            }

            return 0;
        }

        private async Task<(int UpdatesMade, bool? ThresholdReached)> DoTargetUpdates(
            IGitDriver git, RepositoryData repository,
            IReadOnlyCollection<PackageUpdateSet> targetUpdates,
            NuGetSources sources,
            SettingsContainer settings
        )
        {
            if (targetUpdates.Count == 0)
            {
                return (0, null);
            }

            await _solutionRestore.CheckRestore(targetUpdates, settings.WorkingFolder ?? git.WorkingFolder, sources);

            var (updatesDone, thresholdReached) = await _packageUpdater.MakeUpdatePullRequests(git, repository, targetUpdates, sources, settings);

            if (updatesDone < targetUpdates.Count)
            {
                _logger.Minimal($"Attempted {targetUpdates.Count} updates and did {updatesDone}");
            }
            else
            {
                _logger.Normal($"Done {updatesDone} updates");
            }

            return (updatesDone, thresholdReached);
        }

        private static async Task GitInit(IGitDriver git, RepositoryData repository)
        {
            await git.Clone(repository.Pull.Uri, repository.DefaultBranch);
            repository.DefaultBranch = repository.DefaultBranch ?? await git.GetCurrentHead();
            await git.AddRemote(repository.Remote, repository.Push.Uri);
        }
    }
}
