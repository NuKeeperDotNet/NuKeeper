using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Update.Selection;
using NuKeeper.Update;

namespace NuKeeper
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

        public async Task ApplyAnUpdate(IReadOnlyCollection<PackageUpdateSet> updates)
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

            await _updateRunner.Update(filtered.First());
        }
    }
}
