using System;
using System.IO;

namespace NuKeeper
{
    public static class TempFiles
    {
        private static string NuKeeperTempFilesPath()
        {
            return Path.Combine(Path.GetTempPath(), "NuKeeper");
        }

        public static void DeleteExistingTempDirs()
        {
            var dirs = Directory.EnumerateDirectories(NuKeeperTempFilesPath());
            foreach (var dir in dirs)
            {
                TryDelete(dir);
            }
        }

        public static string GetUniqueTemporaryPath()
        {
            var uniqueName = Guid.NewGuid().ToString("N");
            return Path.Combine(NuKeeperTempFilesPath(), uniqueName);
        }

        public static string MakeUniqueTemporaryPath()
        {
            var uniquePath = GetUniqueTemporaryPath();
            Directory.CreateDirectory(uniquePath);
            return uniquePath;
        }

        public static void TryDelete(string tempDir)
        {
            Console.WriteLine($"Attempting delete of temp dir {tempDir}");

            try
            {
                Directory.Delete(tempDir, true);
            }
            catch (Exception)
            {
                Console.WriteLine("Delete failed. Continuing");
            }
        }
    }
}