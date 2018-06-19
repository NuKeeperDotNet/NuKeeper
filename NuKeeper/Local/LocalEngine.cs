using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sort;

namespace NuKeeper.Local
{
    public class LocalEngine
    {
        private readonly IUpdateFinder _updateFinder;
        private readonly IPackageUpdateSetSort _sorter;
        private readonly ILocalUpdater _updater;
        private readonly INuKeeperLogger _logger;

        public LocalEngine(
            IUpdateFinder updateFinder,
            IPackageUpdateSetSort sorter,
            ILocalUpdater updater,
            INuKeeperLogger logger)
        {
            _updateFinder = updateFinder;
            _sorter = sorter;
            _updater = updater;
            _logger = logger;
        }

        public async Task Run(SettingsContainer settings)
        {
            var sortedUpdates = await GetSortedUpdates(settings.UserSettings);

            switch (settings.ModalSettings.Mode)
            {
                case RunMode.Inspect:
                    Report(sortedUpdates);
                    break;

                case RunMode.Update:
                    await _updater.ApplyAnUpdate(sortedUpdates);
                    break;
            }
        }

        private async Task<IReadOnlyCollection<PackageUpdateSet>> GetSortedUpdates(UserSettings settings)
        {
            var folder = TargetFolder(settings);
            var updates = await _updateFinder.FindPackageUpdateSets(
                folder, settings.AllowedChange, settings.NuGetSources);

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
