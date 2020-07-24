using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.Inspection.Files
{
    public class FolderFactory : IFolderFactory
    {
        private const string FolderPrefix = "repo-";
        private readonly INuKeeperLogger _logger;

        public FolderFactory(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Select folders to cleanup at startup
        /// </summary>
        /// <param name="nukeeperTemp">NuKeepers temp folder</param>
        /// <returns></returns>
        public static IEnumerable<DirectoryInfo> GetTempDirsToCleanup(DirectoryInfo nukeeperTemp)
        {
            if (nukeeperTemp == null)
            {
                throw new ArgumentNullException(nameof(nukeeperTemp));
            }

            var dirs = nukeeperTemp.Exists ? nukeeperTemp.EnumerateDirectories() : Enumerable.Empty<DirectoryInfo>();
            var filterDatetime = DateTime.Now.AddHours(-1);
            return dirs.Where(d =>
                d.Name.StartsWith(FolderPrefix, StringComparison.InvariantCultureIgnoreCase) &&
                d.LastWriteTime < filterDatetime);
        }

        public static string NuKeeperTempFilesPath()
        {
            return Path.Combine(Path.GetTempPath(), "NuKeeper");
        }

        /// <summary>
        /// Cleanup folders that are not automatically have been cleaned.
        /// </summary>
        public void DeleteExistingTempDirs()
        {
            var dirInfo = new DirectoryInfo(NuKeeperTempFilesPath());
            foreach (var dir in GetTempDirsToCleanup(dirInfo))
            {
                var folder = new Folder(_logger, dir);
                folder.TryDelete();
            }
        }

        public IFolder FolderFromPath(string folderPath)
        {
            return new Folder(_logger, new DirectoryInfo(folderPath));
        }

        public IFolder UniqueTemporaryFolder()
        {
            var tempDir = new DirectoryInfo(GetUniqueTemporaryPath());
            tempDir.Create();
            return new Folder(_logger, tempDir);
        }

        private static string GetUniqueTemporaryPath()
        {
            var uniqueName = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            return Path.Combine(NuKeeperTempFilesPath(), $"{FolderPrefix}{uniqueName}");
        }
    }
}
