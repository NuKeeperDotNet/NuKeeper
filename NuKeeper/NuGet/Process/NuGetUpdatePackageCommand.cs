using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.ProcessRunner;
using NuKeeper.Types.Logging;

namespace NuKeeper.NuGet.Process
{
    public class NuGetUpdatePackageCommand : IUpdatePackageCommand
    {
        private readonly IExternalProcess _externalProcess;
        private readonly INuKeeperLogger _logger;
        private readonly string[] _sources;

        public NuGetUpdatePackageCommand(
            INuKeeperLogger logger,
            UserSettings settings,
            IExternalProcess externalProcess = null)
        {
            _logger = logger;
            _sources = settings.NuGetSources;
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task Invoke(NuGetVersion newVersion, string packageSource, PackageInProject currentPackage)
        {
            var dirName = currentPackage.Path.Info.DirectoryName;
            var nuget = NuGetPath.FindExecutable();
            var sources = GetSourcesCommandLine(_sources);
            var arguments = $"update packages.config -Id {currentPackage.Id} -Version {newVersion} {sources}";
            _logger.Verbose(arguments);

            await _externalProcess.Run(dirName, nuget, arguments, true);
        }

        private static string GetSourcesCommandLine(IEnumerable<string> sources)
        {
            return sources.Select(s => $"-Source {s}").JoinWithSeparator(" ");
        }
    }
}
