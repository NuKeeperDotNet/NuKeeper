using System;
using System.IO;
using System.Linq;
using NuKeeper.Inspection.Logging;

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
            var uniqueName = Guid.NewGuid().ToString("N");
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
