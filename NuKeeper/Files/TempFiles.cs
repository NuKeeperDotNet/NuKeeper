﻿using System;
using System.IO;
using System.Linq;
using NuKeeper.Logging;

namespace NuKeeper.Files
{
    public static class TempFiles
    {
        private static string NuKeeperTempFilesPath()
        {
            return Path.Combine(Path.GetTempPath(), "NuKeeper");
        }

        public static void DeleteExistingTempDirs(INuKeeperLogger logger)
        {
            var dirInfo = new DirectoryInfo(NuKeeperTempFilesPath());
            var dirs = dirInfo.Exists ? dirInfo.EnumerateDirectories() : Enumerable.Empty<DirectoryInfo>();
            foreach (var dir in dirs)
            {
                TryDelete(dir, logger);
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

        public static void TryDelete(DirectoryInfo tempDir, INuKeeperLogger logger)
        {
            logger.Verbose($"Attempting delete of temp dir {tempDir}");

            try
            {
                tempDir.Delete(true);
            }
            catch (Exception)
            {
                logger.Verbose("Delete failed. Continuing");
            }
        }
    }
}