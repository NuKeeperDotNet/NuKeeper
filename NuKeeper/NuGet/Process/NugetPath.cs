using System;
using System.IO;
using System.Linq;

namespace NuKeeper.NuGet.Process
{
    public static class NugetPath
    {
        public static string Find()
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
