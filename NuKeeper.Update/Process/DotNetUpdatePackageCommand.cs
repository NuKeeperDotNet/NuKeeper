using System.Linq;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Update.ProcessRunner;

namespace NuKeeper.Update.Process
{
    public class DotNetUpdatePackageCommand : IPackageCommand
    {
        private readonly IExternalProcess _externalProcess;
        private readonly INuKeeperLogger _logger;
        private readonly NuGetSources _sources;

        public DotNetUpdatePackageCommand(
            INuKeeperLogger logger,
            NuGetSources sources,
            IExternalProcess externalProcess = null)
        {
            _logger = logger;
            _sources = sources;
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task Invoke(NuGetVersion newVersion, string packageSource, PackageInProject currentPackage)
        {
            var projectPath = currentPackage.Path.Info.DirectoryName;
            var projectFileName = currentPackage.Path.Info.Name;

            var sources = _sources.CommandLine("-s");

            _logger.Verbose($"dotnet update package {currentPackage.Id} in path {projectPath} {projectFileName} from sources {sources}");

            await _externalProcess.Run(projectPath, "dotnet", $"restore {projectFileName} {sources}", true);
            await _externalProcess.Run(projectPath, "dotnet", $"remove {projectFileName} package {currentPackage.Id}", true);
            await _externalProcess.Run(projectPath, "dotnet", $"add {projectFileName} package {currentPackage.Id} -v {newVersion} -s {packageSource}", true);
        }
    }
}
