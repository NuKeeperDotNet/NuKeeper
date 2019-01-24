using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection
{
    public class UpdateFinder: IUpdateFinder
    {
        private readonly IRepositoryScanner _repositoryScanner;
        private readonly IPackageUpdatesLookup _packageUpdatesLookup;
        private readonly INuKeeperLogger _logger;

        // ASP.NET Core has well known metapackages that should be implicitly versioned
        // based on installed runtime
        private static readonly IReadOnlyCollection<string> KnownMetapackage = new List<string>
            {
                "Microsoft.AspNetCore.App",
                "Microsoft.AspNetCore.All"
            };

        public UpdateFinder(
            IRepositoryScanner repositoryScanner,
            IPackageUpdatesLookup packageUpdatesLookup,
            INuKeeperLogger logger)
        {
            _repositoryScanner = repositoryScanner;
            _packageUpdatesLookup = packageUpdatesLookup;
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<PackageUpdateSet>> FindPackageUpdateSets(
            IFolder workingFolder,
            NuGetSources sources,
            VersionChange allowedChange,
            UsePrerelease usePrerelease,
            Regex includes = null,
            Regex excludes = null)
        {
            var packages = FindPackages(workingFolder);

            _logger.Normal($"Found {packages.Count} packages");

            var filtered = FilteredByIncludeExclude(packages, includes, excludes);

            _logger.Log(PackagesFoundLogger.Log(filtered));

            // look for updates to these packages
            var updates = await _packageUpdatesLookup.FindUpdatesForPackages(
                filtered, sources, allowedChange, usePrerelease);

            _logger.Log(UpdatesLogger.Log(updates));
            return updates;
        }


        private IReadOnlyCollection<PackageInProject> FilteredByIncludeExclude(IReadOnlyCollection<PackageInProject> all, Regex includes, Regex excludes)
        {
            var filteredByIncludeExclude = all
                .Where(package => RegexMatch.IncludeExclude(package.Id,includes, excludes))
                .ToList();

            if (filteredByIncludeExclude.Count < all.Count)
            {
                var filterDesc = string.Empty;
                if (excludes != null)
                {
                    filterDesc += $"Exclude '{excludes}'";
                }

                if (includes != null)
                {
                    filterDesc += $"Include '{includes}'";
                }

                _logger.Normal($"Filtered by {filterDesc} from {all.Count} to {filteredByIncludeExclude.Count}");
            }

            return filteredByIncludeExclude;
        }

        private IReadOnlyCollection<PackageInProject> FindPackages(IFolder workingFolder)
        {
            // scan for nuget packages
            var allPackages = _repositoryScanner.FindAllNuGetPackages(workingFolder);

            var metaPackages = allPackages
                .Where(x => KnownMetapackage.Contains(x.Id, StringComparer.OrdinalIgnoreCase));

            foreach (var metaPackage in metaPackages)
            {
                LogVersionedMetapackage(metaPackage);
            }

            return allPackages.Except(metaPackages)
                .ToList();
        }

        private void LogVersionedMetapackage(PackageInProject metaPackage)
        {
            _logger.Error($"Metapackage '{metaPackage.Id}' has version {metaPackage.Version} in {metaPackage.Path.FullName}, " +
                  "but should not have explicit version.");
        }
    }
}
