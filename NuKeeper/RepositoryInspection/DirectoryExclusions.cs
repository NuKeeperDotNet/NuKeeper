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
            // subDir is a full path, check all parts
            return ExcludedDirNames.Any(s => PathContains(path, s));
        }

        private static bool PathContains(string fullPath, string dirName)
        {
            var dirInPath = Path.DirectorySeparatorChar + dirName + Path.DirectorySeparatorChar;

            return 
                !string.IsNullOrEmpty(fullPath) &&
                 (fullPath.IndexOf(dirInPath, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }

        private static readonly List<string> ExcludedDirNames = new List<string>
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
