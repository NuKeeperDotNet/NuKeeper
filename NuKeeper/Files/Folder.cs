using System;
using System.Collections.Generic;
using System.IO;
using NuKeeper.Logging;

namespace NuKeeper.Files
{
    public class Folder : IFolder
    {
        private readonly INuKeeperLogger _logger;
        private readonly DirectoryInfo _root;

        public Folder(INuKeeperLogger logger, DirectoryInfo root)
        {
            _logger = logger;
            _root = root;
        }

        public string FullPath => _root.FullName;

        public IEnumerable<FileInfo> Find(string pattern)
        {
            return _root.EnumerateFiles(pattern, SearchOption.AllDirectories);
        }

        public void TryDelete()
        {
            _logger.Verbose($"Attempting delete of working folder {_root.FullName}");

            try
            {
                _root.Delete(true);
            }
            catch (Exception ex)
            {
                _logger.Verbose($"Folder delete failed: {ex.Message}");
            }
        }
    }
}
