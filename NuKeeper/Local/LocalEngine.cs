using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Creators;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sort;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Local
{
    public class LocalEngine
    {
        private readonly INuGetSourcesReader _nuGetSourcesReader;
        private readonly IUpdateFinder _updateFinder;
        private readonly IPackageUpdateSetSort _sorter;
        private readonly ICreate<ILocalUpdater> _updaterCreator;
        private readonly INuKeeperLogger _logger;

        public LocalEngine(
            INuGetSourcesReader nuGetSourcesReader,
            IUpdateFinder updateFinder,
            IPackageUpdateSetSort sorter,
            ICreate<ILocalUpdater> updaterCreator,
            INuKeeperLogger logger)
        {
            _nuGetSourcesReader = nuGetSourcesReader;
            _updateFinder = updateFinder;
            _sorter = sorter;
            _updaterCreator = updaterCreator;
            _logger = logger;
        }

        public async Task Run(SettingsContainer settings)
        {
            var updater = _updaterCreator.Create(settings);
            var folder = TargetFolder(settings.UserSettings);

            var sources = _nuGetSourcesReader.Read(folder, settings.UserSettings.NuGetSources);

            var sortedUpdates = await GetSortedUpdates(folder, sources, settings.UserSettings.AllowedChange);

            switch (settings.ModalSettings.Mode)
            {
                case RunMode.Inspect:
                    Report(sortedUpdates);
                    break;

                case RunMode.Update:
                    await updater.ApplyAnUpdate(sortedUpdates, sources);
                    break;
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

        private static void Report(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            var reporter = new ConsoleReporter();
            reporter.Report("ConsoleReport", updates);
        }
    }
}
