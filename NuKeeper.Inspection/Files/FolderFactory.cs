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
        private readonly string _systemTempPath;

        public FolderFactory(INuKeeperLogger logger)
        {
            _logger = logger;
            _systemTempPath = Path.GetTempPath();
        }

        public IFolder UniqueTemporaryFolder()
        {
            var tempDir = new DirectoryInfo(GetUniqueTemporaryPath());
            tempDir.Create();
            return new Folder(_logger, tempDir);
        }

        private string NuKeeperTempFilesPath()
        {
            return Path.Combine(_systemTempPath, "NuKeeper");
        }

        private string GetUniqueTemporaryPath()
        {
            var uniqueName = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            return Path.Combine(NuKeeperTempFilesPath(), uniqueName);
        }

        public void DeleteExistingTempDirs()
        {
            var dirInfo = new DirectoryInfo(NuKeeperTempFilesPath());
            var dirs = dirInfo.Exists ? dirInfo.EnumerateDirectories() : Enumerable.Empty<DirectoryInfo>();
            foreach (var dir in dirs)
            {
                var folder = new Folder(_logger, dir);
                folder.TryDelete();
            }
        }
    }
}
