using NuKeeper.Inspection;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.Report;
using System.IO;
using System.Threading.Tasks;

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

        public async Task Run()
        {
            var updates = await _updateFinder.FindPackageUpdateSets(CurrentFolder());

            var reporter = new ConsoleReporter();
            reporter.Report("ConsoleReport", updates);
        }

        private IFolder CurrentFolder()
        {
            var dir = Directory.GetCurrentDirectory();
            return new Folder(_logger, new DirectoryInfo(dir));
        }
    }
}
