using System;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Versioning;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Update.ProcessRunner;

namespace NuKeeper.Update.Process
{
    public class DotNetUpdatePackageCommand : IDotNetUpdatePackageCommand
    {
        private readonly IExternalProcess _externalProcess;

        public DotNetUpdatePackageCommand(IExternalProcess externalProcess)
        {
            _externalProcess = externalProcess;
        }

        public async Task Invoke(PackageInProject currentPackage,
            NuGetVersion newVersion, PackageSource packageSource, NuGetSources allSources)
        {
            var projectPath = currentPackage.Path.Info.DirectoryName;
            var projectFileName = currentPackage.Path.Info.Name;
            var sourceUrl = EscapedUri(packageSource.SourceUri);
            var sources = allSources.CommandLine("-s");

            var restoreCommand = $"restore {projectFileName} {sources}";
            await _externalProcess.Run(projectPath, "dotnet", restoreCommand, true);

            if (currentPackage.Path.PackageReferenceType == PackageReferenceType.ProjectFileOldStyle)
            {
                var removeCommand = $"remove {projectFileName} package {currentPackage.Id}";
                await _externalProcess.Run(projectPath, "dotnet", removeCommand, true);
            }

            var addCommand = $"add {projectFileName} package {currentPackage.Id} -v {newVersion} -s {sourceUrl}";
            await _externalProcess.Run(projectPath, "dotnet", addCommand, true);
        }

        private static string EscapedUri(Uri uri)
        {
            if (uri == null)
            {
                return string.Empty;
            }

            var result = uri.ToString();

            if (result.IndexOf(" ", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Uri.EscapeUriString(result);
            }

            return result;
        }
    }
}
