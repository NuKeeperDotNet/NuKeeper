using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Versioning;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update.ProcessRunner;

namespace NuKeeper.Update.Process
{
    public class DotNetUpdatePackageCommand : IPackageCommand
    {
        private readonly IExternalProcess _externalProcess;
        private readonly INuKeeperLogger _logger;

        public DotNetUpdatePackageCommand(
            INuKeeperLogger logger,
            IExternalProcess externalProcess = null)
        {
            _logger = logger;
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task Invoke(PackageInProject currentPackage,
            NuGetVersion newVersion, PackageSource packageSource, NuGetSources allSources)
        {
            var projectPath = currentPackage.Path.Info.DirectoryName;
            var projectFileName = currentPackage.Path.Info.Name;
            var sourceUrl = packageSource.SourceUri.ToString();
            var sources = allSources.CommandLine("-s");

            _logger.Verbose($"dotnet update package {currentPackage.Id} in path {projectPath} {projectFileName} from source {sourceUrl}");

            await _externalProcess.Run(projectPath, "dotnet", $"restore {projectFileName} {sources}", true);
            await _externalProcess.Run(projectPath, "dotnet", $"remove {projectFileName} package {currentPackage.Id}", true);
            await _externalProcess.Run(projectPath, "dotnet", $"add {projectFileName} package {currentPackage.Id} -v {newVersion} -s {sourceUrl}", true);
        }
    }
}
