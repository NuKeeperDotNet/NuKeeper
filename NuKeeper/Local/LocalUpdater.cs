using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Inspection.Report.Formats;
using NuKeeper.Update;
using NuKeeper.Update.Process;
using NuKeeper.Update.Selection;

namespace NuKeeper.Local
{
    public class LocalUpdater : ILocalUpdater
    {
        private readonly IUpdateSelection _selection;
        private readonly IUpdateRunner _updateRunner;
        private readonly ISolutionRestore _solutionRestore;
        private readonly INuKeeperLogger _logger;

        public LocalUpdater(
            IUpdateSelection selection,
            IUpdateRunner updateRunner,
            ISolutionRestore solutionRestore,
            INuKeeperLogger logger)
        {
            _selection = selection;
            _updateRunner = updateRunner;
            _solutionRestore = solutionRestore;
            _logger = logger;
        }

        public async Task ApplyUpdates(
            IReadOnlyCollection<PackageUpdateSet> updates,
            IFolder workingFolder,
            NuGetSources sources,
            SettingsContainer settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (!updates.Any())
            {
                return;
            }

            var filtered = _selection
                .Filter(updates, settings.PackageFilters);

            if (!filtered.Any())
            {
                _logger.Detailed("All updates were filtered out");
                return;
            }

            await ApplyUpdates(filtered, workingFolder, sources);
        }

        private async Task ApplyUpdates(IReadOnlyCollection<PackageUpdateSet> updates, IFolder workingFolder, NuGetSources sources)
        {
            await _solutionRestore.CheckRestore(updates, workingFolder, sources);

            foreach (var update in updates)
            {
                _logger.Minimal("Updating " + Description.ForUpdateSet(update));

                await _updateRunner.Update(update, sources);
            }
        }
    }
}
