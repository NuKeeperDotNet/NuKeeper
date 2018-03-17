using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.Configuration;
using NuKeeper.Logging;
using NuKeeper.ProcessRunner;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Process
{
    public class DotNetUpdatePackageCommand : IUpdatePackageCommand
    {
        private readonly INuKeeperLogger _logger;
        private readonly IExternalProcess _externalProcess;
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

            await _externalProcess.Run(dirName, "dotnet", $"restore {sources}", true);
            await _externalProcess.Run(dirName, "dotnet", $"remove package {currentPackage.Id}", true);
            await _externalProcess.Run(dirName, "dotnet", $"add package {currentPackage.Id} -v {newVersion} -s {packageSource}", true);
        }

        private static string GetSourcesCommandLine(IEnumerable<string> sources)
        {
            return sources.Select(s => $"-s {s}").JoinWithSeparator(" ");
        }
    }
}
