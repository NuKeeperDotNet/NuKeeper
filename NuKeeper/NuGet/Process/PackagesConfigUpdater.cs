using NuKeeper.ProcessRunner;
using System;
using System.IO;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Process
{
    public class PackagesConfigUpdater : INuGetUpdater
    {
        private readonly IExternalProcess _externalProcess;

        public PackagesConfigUpdater(IExternalProcess externalProcess = null)
        {
            _externalProcess = externalProcess ?? new ExternalProcess();
        }

        public async Task UpdatePackage(NuGetVersion newVersion, PackageInProject currentPackage)
        {
            var dirName = currentPackage.Path.FullDirectory;
            var nuget = GetNuGetPath();
            var updateCommand = $"cd {dirName}"
                + $" & {nuget} restore packages.config"
                + $" & {nuget} update packages.config -Id {currentPackage.Id} -Version {newVersion}";
            Console.WriteLine(updateCommand);
            await RunExternalCommand(updateCommand);
        }

        private async Task RunExternalCommand(string command)
        {
            var result = await _externalProcess.Run(command);

            if (!result.Success)
            {
                throw new Exception($"Exit code: {result.ExitCode}\n\n{result.Output}\n\n{result.ErrorOutput}");
            }
        }

        private string GetNuGetPath()
        {
            var profile = Environment.GetEnvironmentVariable("userprofile");

            return Path.GetFullPath(Path.Combine(profile, ".nuget\\packages\\nuget.commandline\\4.1.0\\tools\\NuGet.exe"));
        }
    }
}
