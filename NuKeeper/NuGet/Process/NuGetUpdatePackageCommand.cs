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
    public class NuGetUpdatePackageCommand : IUpdatePackageCommand
    {
        private readonly IExternalProcess _externalProcess;
        private readonly INuKeeperLogger _logger;
        private readonly string[] _sources;

        public NuGetUpdatePackageCommand(
            INuKeeperLogger logger,
            UserPreferences settings,
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
            var updateCommand = $"cd {dirName}" + 
                $" & {nuget} update packages.config -Id {currentPackage.Id} -Version {newVersion} {sources}";
            _logger.Verbose(updateCommand);

            await _externalProcess.Run(updateCommand, true);
        }

        private static string GetSourcesCommandLine(IEnumerable<string> sources)
        {
            return sources.Select(s => $"-Source {s}").JoinWithSeparator(" ");
        }
    }
}
