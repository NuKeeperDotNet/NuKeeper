using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task ApplyAnUpdate(
            IReadOnlyCollection<PackageUpdateSet> updates,
            NuGetSources sources)
        {
            if (!updates.Any())
            {
                return;
            }

            var filtered = await _selection
                .Filter(updates, p => Task.FromResult(true));

            if (!filtered.Any())
            {
                _logger.Verbose("All updates were filtered out");
                return;
            }

            var candidate = filtered.First();

            var reporter = new ConsoleReporter();
            _logger.Terse("Updating " + reporter.Describe(candidate));

            await _updateRunner.Update(candidate, sources);
        }
    }
}
