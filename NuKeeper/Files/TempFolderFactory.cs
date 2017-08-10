using System.IO;
using NuKeeper.Logging;

namespace NuKeeper.Files
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
            var tempDir = new DirectoryInfo(TempFiles.GetUniqueTemporaryPath());
            tempDir.Create();
            return new Folder(_logger, tempDir);
        }
    }
}