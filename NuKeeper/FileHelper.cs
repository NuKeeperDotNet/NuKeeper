using System;
using System.IO;

namespace NuKeeper
{
    public static class FileHelper
    {
        public static string GetUniqueTemporaryPath()
        {
            var uniqueName = Guid.NewGuid().ToString("N");
            return Path.Combine(Path.GetTempPath(), "NuKeeper", uniqueName);
        }

        public static string MakeUniqueTemporaryPath()
        {
            var uniquePath = GetUniqueTemporaryPath();
            Directory.CreateDirectory(uniquePath);
            return uniquePath;
        }
    }
}