using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.ProcessRunner;

namespace NuKeeper.NuGet.Process
{
    public class DotNetUpdatePackageCommand : IUpdatePackageCommand
    {
        private readonly IExternalProcess _externalProcess;
        private readonly string[] _sources;

        public DotNetUpdatePackageCommand(
            UserSettings settings,
            IExternalProcess externalProcess = null)
        {
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
