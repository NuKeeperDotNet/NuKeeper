using System;
using System.Globalization;
using System.IO;
using System.Linq;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.Inspection.Files
{
    public class FolderFactory : IFolderFactory
    {
        private readonly INuKeeperLogger _logger;

        public FolderFactory(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public IFolder UniqueTemporaryFolder()
        {
            var tempDir = new DirectoryInfo(GetUniqueTemporaryPath());
            tempDir.Create();
            return new Folder(_logger, tempDir);
        }

        private static string NuKeeperTempFilesPath()
        {
            return Path.Combine(Path.GetTempPath(), "NuKeeper");
        }

        private static string GetUniqueTemporaryPath()
        {
            var uniqueName = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            return Path.Combine(NuKeeperTempFilesPath(), uniqueName);
        }

        /// <summary>
        /// Cleanup folders that are not automatically have been cleaned.
        /// Only delete folders older than 1 hour to 
        /// </summary>
        public void DeleteExistingTempDirs()
        {
            var dirInfo = new DirectoryInfo(NuKeeperTempFilesPath());
            var dirs = dirInfo.Exists ? dirInfo.EnumerateDirectories() : Enumerable.Empty<DirectoryInfo>();
            var filterDatetime = DateTime.Now.AddHours(-1);
            foreach (var dir in dirs.Where(d => d.LastWriteTime < filterDatetime))
            {
                var folder = new Folder(_logger, dir);
                folder.TryDelete();
            }
        }
    }
}
