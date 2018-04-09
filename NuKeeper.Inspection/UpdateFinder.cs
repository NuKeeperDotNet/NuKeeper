using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Types.Files;
using NuKeeper.Types.Logging;

namespace NuKeeper.Inspection
{
    public class UpdateFinder
    {
        private readonly IRepositoryScanner _repositoryScanner;
        private readonly IPackageUpdatesLookup _packageUpdatesLookup;
        private readonly INuKeeperLogger _logger;

        public UpdateFinder(
            IRepositoryScanner repositoryScanner,
            IPackageUpdatesLookup packageUpdatesLookup,
            INuKeeperLogger logger)
        {
            _repositoryScanner = repositoryScanner;
            _packageUpdatesLookup = packageUpdatesLookup;
            _logger = logger;
        }

        public async Task<List<PackageUpdateSet>> FindPackageUpdateSets(IFolder workingFolder)
        {
            // scan for nuget packages
            var packages = _repositoryScanner.FindAllNuGetPackages(workingFolder)
                .ToList();

            _logger.Log(PackagesFoundLogger.Log(packages));

            // look for updates to these packages
            var updates = await _packageUpdatesLookup.FindUpdatesForPackages(packages);
            _logger.Log(UpdatesLogger.Log(updates));
            return updates;
        }
    }
}
