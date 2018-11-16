using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sort;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Local
{
    public class LocalEngine : ILocalEngine
    {
        private readonly INuGetSourcesReader _nuGetSourcesReader;
        private readonly IUpdateFinder _updateFinder;
        private readonly IPackageUpdateSetSort _sorter;
        private readonly ILocalUpdater _updater;
        private readonly IReporter _reporter;
        private readonly INuKeeperLogger _logger;

        public LocalEngine(
            INuGetSourcesReader nuGetSourcesReader,
            IUpdateFinder updateFinder,
            IPackageUpdateSetSort sorter,
            ILocalUpdater updater,
            IReporter reporter,
            INuKeeperLogger logger)
        {
            _nuGetSourcesReader = nuGetSourcesReader;
            _updateFinder = updateFinder;
            _sorter = sorter;
            _updater = updater;
            _reporter = reporter;
            _logger = logger;
        }

        public async Task Run(SettingsContainer settings, bool write)
        {
            var folder = TargetFolder(settings.UserSettings);

            var sources = _nuGetSourcesReader.Read(folder, settings.UserSettings.NuGetSources);

            var sortedUpdates = await GetSortedUpdates(folder, sources, settings.UserSettings.AllowedChange);

            Report(settings.UserSettings, sortedUpdates);

            if (write)
            {
                await _updater.ApplyUpdates(sortedUpdates, folder, sources, settings);
            }
        }

        private async Task<IReadOnlyCollection<PackageUpdateSet>> GetSortedUpdates(
            IFolder folder,
            NuGetSources sources,
            VersionChange allowedChange)
        {
            var updates = await _updateFinder.FindPackageUpdateSets(
                folder, sources, allowedChange);

            return _sorter.Sort(updates)
                .ToList();
        }

        private IFolder TargetFolder(UserSettings settings)
        {
            string dir = settings.Directory;
            if (string.IsNullOrWhiteSpace(dir))
            {
                dir = Directory.GetCurrentDirectory();
            }

            return new Folder(_logger, new DirectoryInfo(dir));
        }

        private void Report(
            UserSettings settings,
            IReadOnlyCollection<PackageUpdateSet> updates)
        {
            _reporter.Report(
                settings.OutputDestination, settings.OutputFormat,
                "Inspect", settings.OutputFileName, updates);
        }
    }
}
