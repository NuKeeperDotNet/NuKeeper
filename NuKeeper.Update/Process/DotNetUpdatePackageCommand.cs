using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
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
            if (currentPackage == null)
            {
                throw new ArgumentNullException(nameof(currentPackage));
            }

            if (packageSource == null)
            {
                throw new ArgumentNullException(nameof(packageSource));
            }

            if (allSources == null)
            {
                throw new ArgumentNullException(nameof(allSources));
            }

            var projectPath = currentPackage.Path.Info.DirectoryName;
            var projectFileName = currentPackage.Path.Info.Name;
            var sourceUrl = UriEscapedForArgument(packageSource.SourceUri);
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

        private static string UriEscapedForArgument(Uri uri)
        {
            if (uri == null)
            {
                return string.Empty;
            }

            return ArgumentEscaper.EscapeAndConcatenate(new string[] { uri.ToString() });
        }
    }
}
