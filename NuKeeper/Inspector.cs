using System.Collections.Generic;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.Report;
using System.IO;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper
{
    public class Inspector
    {
        private readonly IUpdateFinder _updateFinder;
        private readonly INuKeeperLogger _logger;

        public Inspector(IUpdateFinder updateFinder, INuKeeperLogger logger)
        {
            _updateFinder = updateFinder;
            _logger = logger;
        }

        public async Task Run(UserSettings settings)
        {
            var folder = TargetFolder(settings);
            var updates = await _updateFinder.FindPackageUpdateSets(folder);
            Report(updates);
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
