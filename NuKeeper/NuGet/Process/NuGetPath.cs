using System;
using System.IO;
using System.Linq;

namespace NuKeeper.NuGet.Process
{
    public static class NuGetPath
    {
        public static string FindExecutable()
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

        private static string FindNugetInPackagesUnderProfile()
        {
            var profile = Environment.GetEnvironmentVariable("userprofile");

            if (string.IsNullOrWhiteSpace(profile))
            {
                throw new Exception("Could not find user profile path");
            }

            var commandlinePackageDir = Path.Combine(profile, ".nuget\\packages\\nuget.commandline");

            if (!Directory.Exists(commandlinePackageDir))
            {
                throw new Exception("Could not find nuget commandline path: " + commandlinePackageDir);
            }

            var highestVersion = Directory.GetDirectories(commandlinePackageDir)
                .OrderByDescending(n => n)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(highestVersion))
            {
                throw new Exception("Could not find a version of nuget.commandline");
            }

            var nugetProgramPath = Path.Combine(highestVersion, "tools\\NuGet.exe");
            return Path.GetFullPath(nugetProgramPath);
        }
    }
}
