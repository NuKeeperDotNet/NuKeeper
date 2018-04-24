using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.ProcessRunner;

namespace NuKeeper.NuGet.Process
{
    public class DotNetUpdatePackageCommand : IUpdatePackageCommand
    {
        private readonly IExternalProcess _externalProcess;
        private readonly INuKeeperLogger _logger;
        private readonly string[] _sources;

        public DotNetUpdatePackageCommand(
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
            var sources = GetSourcesCommandLine(_sources);

            _logger.Verbose($"dotnet update package {currentPackage.Id} in path {dirName} {currentPackage.Path.RelativePath} from sources {sources}");

            await _externalProcess.Run(dirName, "dotnet", $"restore {sources} {currentPackage.Path.RelativePath}", true);
            await _externalProcess.Run(dirName, "dotnet", $"remove {currentPackage.Path.RelativePath} package {currentPackage.Id}", true);
            await _externalProcess.Run(dirName, "dotnet", $"add {currentPackage.Path.RelativePath} package {currentPackage.Id} -v {newVersion} -s {packageSource}", true);
        }

        private static string GetSourcesCommandLine(IEnumerable<string> sources)
        {
            return sources.Select(s => $"-s {s}").JoinWithSeparator(" ");
        }
    }
}
