using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Creators;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update;
using NuKeeper.Update.Selection;

namespace NuKeeper.Local
{
    public class LocalUpdater : ILocalUpdater
    {
        private readonly IUpdateSelection _selection;
        private readonly IUpdateRunner _updateRunner;
        private readonly INuKeeperLogger _logger;

        public LocalUpdater(
            IUpdateSelection selection,
            IUpdateRunner updateRunner,
            INuKeeperLogger logger)
        {
            _selection = selection;
            _updateRunner = updateRunner;
            _logger = logger;
        }

        public async Task ApplyUpdates(
            IReadOnlyCollection<PackageUpdateSet> updates,
            NuGetSources sources,
            SettingsContainer settings)
        {
            if (!updates.Any())
            {
                return;
            }

            var filterSettings = FilterSettingsCreator.MakeFilterSettings(settings.UserSettings);

            var filtered = await _selection
                .Filter(updates, filterSettings, p => Task.FromResult(true));

            if (!filtered.Any())
            {
                _logger.Detailed("All updates were filtered out");
                return;
            }

            foreach (var update in filtered)
            {
                var reporter = new ConsoleReporter();
                _logger.Minimal("Updating " + reporter.Describe(update));

                await _updateRunner.Update(update, sources);
            }
        }
    }
}
