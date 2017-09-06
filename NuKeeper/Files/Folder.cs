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
            _logger.Verbose($"Attempting delete of folder {_root.FullName}");

            try
            {
                DeleteDirectoryInternal(_root.FullName);
                _logger.Verbose($"Deleted folder {_root.FullName}");
            }
            catch (Exception ex)
            {
                _logger.Verbose($"Folder delete failed: {ex.GetType().Name} {ex.Message}");
            }
        }

        /// <summary>
        /// https://stackoverflow.com/questions/1157246/unauthorizedaccessexception-trying-to-delete-a-file-in-a-folder-where-i-can-dele
        /// </summary>
        /// <param name="targetDir"></param>
        private void DeleteDirectoryInternal(string targetDir)
        {
            // remove any "read-only" flag that would prevent the delete
            File.SetAttributes(targetDir, FileAttributes.Normal);

            var files = Directory.GetFiles(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            var subDirs = Directory.GetDirectories(targetDir);
            foreach (string dir in subDirs)
            {
                DeleteDirectoryInternal(dir);
            }

            Directory.Delete(targetDir, false);
        }
    }
}
