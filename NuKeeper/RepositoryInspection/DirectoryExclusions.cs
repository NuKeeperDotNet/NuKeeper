using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NuKeeper.RepositoryInspection
{
    public static class DirectoryExclusions
    {
        public static bool PathIsExcluded(string path)
        {
            // subDir is a full path, check the last part
            var dirName = new DirectoryInfo(path).Name;
            return IsExcluded(dirName);
        }

        private static bool IsExcluded(string dirName)
        {
            return _excludedDirNames.Any(s => string.Equals(s, dirName, StringComparison.OrdinalIgnoreCase));
        }

        private static readonly List<string> _excludedDirNames = new List<string>
        {
            ".git",
            ".vs",
            "obj",
            "bin",
            "node_modules",
            "packages"
        };
    }
}
