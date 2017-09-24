using System;
using System.Collections.Generic;
using System.Linq;

namespace NuKeeper.RepositoryInspection
{
    public static class DirectoryExclusions
    {
        public static bool PathIsExcluded(string path)
        {
            // subDir is a full path, check all parts
            return ExcludedDirNames.Any(s => StringContains(path, s));
        }

        private static bool StringContains(string fullPath, string test)
        {
            var testInPath = "\\" + test + "\\";

            return 
                !string.IsNullOrEmpty(fullPath) &&
                 (fullPath.IndexOf(testInPath, StringComparison.InvariantCultureIgnoreCase) >= 0);
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
