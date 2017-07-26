using System;
using System.IO;

namespace NuKeeper.Nuget.Process
{
    public static class NugetPath
    {
        public static string Find()
        {
            var profile = Environment.GetEnvironmentVariable("userprofile");
            var nugetDir = Path.Combine(profile, ".nuget\\packages\\nuget.commandline\\4.1.0\\tools");

            if (!Directory.Exists(nugetDir))
            {
                throw new Exception("Could not find nuget commandline path: " + nugetDir);
            }

            return Path.GetFullPath(Path.Combine(nugetDir, "NuGet.exe"));
        }
    }
}
