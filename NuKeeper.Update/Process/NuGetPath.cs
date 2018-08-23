using System;
using System.IO;
using System.Linq;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Update.Process
{
    public class NuGetPath : INuGetPath
    {
        private readonly INuKeeperLogger _logger;
        private readonly Lazy<string> _executablePath;

        public NuGetPath(INuKeeperLogger logger)
        {
            _logger = logger;
            this._executablePath = new Lazy<string>(FindExecutable);
        }

        public string Executable => _executablePath.Value;

        private string FindExecutable()
        {
            var localNugetPath = FindLocalNuget();

            if (!string.IsNullOrEmpty(localNugetPath))
            {
                return localNugetPath;
            }

            return FindNugetInPackagesUnderProfile();
        }

        private string FindLocalNuget()
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var fullPath = Path.Combine(appDir, "NuGet.exe");
            if (File.Exists(fullPath))
            {
                _logger.Detailed("Found NuGet.exe: " + fullPath);
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
                return string.Empty;
            }

            var commandlinePackageDir = Path.Combine(profile, ".nuget", "packages", "nuget.commandline");
            _logger.Detailed("Checking for NuGet.exe in packages directory: " + commandlinePackageDir);

            if (!Directory.Exists(commandlinePackageDir))
            {
                _logger.Error("Could not find nuget commandline path: " + commandlinePackageDir);
                return string.Empty;
            }

            var highestVersion = Directory.GetDirectories(commandlinePackageDir)
                .OrderByDescending(n => n)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(highestVersion))
            {
                _logger.Error("Could not find a version of nuget.commandline");
                return string.Empty;
            }

            var nugetProgramPath = Path.Combine(highestVersion, "tools", "NuGet.exe");
            var fullPath = Path.GetFullPath(nugetProgramPath);
            _logger.Detailed("Found NuGet.exe: " + fullPath);

            return fullPath;
        }
    }
}
