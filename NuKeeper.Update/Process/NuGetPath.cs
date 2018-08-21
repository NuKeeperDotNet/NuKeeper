using System;
using System.IO;
using System.Linq;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Update.Process
{
    public class NuGetPath
    {
        private readonly INuKeeperLogger _logger;

        public NuGetPath(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public string FindExecutable()
        {
            var localNugetPath = FindLocalNuget();

            if (!string.IsNullOrEmpty(localNugetPath))
            {
                return localNugetPath;
            }


            return FindNugetInPackagesUnderProfile();
        }

        private static string FindLocalNuget()
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var fullPath = Path.Combine(appDir, "NuGet.exe");
            if (File.Exists(fullPath))
            {
                return fullPath;
            }

            return string.Empty;
        }

        private string FindNugetInPackagesUnderProfile()
        {
            var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (string.IsNullOrWhiteSpace(profile))
            {
                _logger.Error("Could not find user profile path");
                return String.Empty;
            }

            var commandlinePackageDir = Path.Combine(profile, ".nuget", "packages", "nuget.commandline");

            if (!Directory.Exists(commandlinePackageDir))
            {
                _logger.Error("Could not find nuget commandline path: " + commandlinePackageDir);
                return String.Empty;
            }

            var highestVersion = Directory.GetDirectories(commandlinePackageDir)
                .OrderByDescending(n => n)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(highestVersion))
            {
                _logger.Error("Could not find a version of nuget.commandline");
                return String.Empty;
            }

            var nugetProgramPath = Path.Combine(highestVersion, "tools", "NuGet.exe");
            return Path.GetFullPath(nugetProgramPath);
        }
    }
}
