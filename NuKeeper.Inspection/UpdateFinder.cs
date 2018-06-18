using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Inspection
{
    public class UpdateFinder: IUpdateFinder
    {
        private readonly IRepositoryScanner _repositoryScanner;
        private readonly IPackageUpdatesLookup _packageUpdatesLookup;
        private readonly INuKeeperLogger _logger;
        private readonly INugetSourcesFactory _nugetSourcesFactory;

        public UpdateFinder(
            IRepositoryScanner repositoryScanner,
            IPackageUpdatesLookup packageUpdatesLookup,
            INugetSourcesFactory nugetSourcesFactory,
            INuKeeperLogger logger)
        {
            _repositoryScanner = repositoryScanner;
            _packageUpdatesLookup = packageUpdatesLookup;
            _nugetSourcesFactory = nugetSourcesFactory;
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<PackageUpdateSet>> FindPackageUpdateSets(
            IFolder workingFolder, VersionChange allowedChange)
        {
            // scan for nuget packages
            var packages = _repositoryScanner.FindAllNuGetPackages(workingFolder)
                .ToList();

            _logger.Log(PackagesFoundLogger.Log(packages));

            var nugetSources = _nugetSourcesFactory.ReadNugetSources(workingFolder);

            // look for updates to these packages
            var updates = await _packageUpdatesLookup.FindUpdatesForPackages(
                packages, nugetSources, allowedChange);

            _logger.Log(UpdatesLogger.Log(updates));
            return updates;
        }
    }
}
