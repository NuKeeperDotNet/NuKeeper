using System.Collections.Generic;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.Report;
using System.IO;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sort;
using System.Linq;

namespace NuKeeper
{
    public class Inspector
    {
        private readonly IUpdateFinder _updateFinder;
        private readonly IPackageUpdateSetSort _sorter;
        private readonly INuKeeperLogger _logger;

        public Inspector(
            IUpdateFinder updateFinder,
            IPackageUpdateSetSort sorter,
            INuKeeperLogger logger)
        {
            _updateFinder = updateFinder;
            _sorter = sorter;
            _logger = logger;
        }

        public async Task Run(UserSettings settings)
        {
            var folder = TargetFolder(settings);
            var updates = await _updateFinder.FindPackageUpdateSets(folder);

            var sortedUpdates = _sorter.Sort(updates)
                .ToList();

            Report(sortedUpdates);
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

        private static void Report(List<PackageUpdateSet> updates)
        {
            var reporter = new ConsoleReporter();
            reporter.Report("ConsoleReport", updates);
        }

    }
}
