using System;
using System.IO;
using System.Linq;

namespace NuKeeper.RepositoryInspection
{
    public class SolutionFinder
    {
        public string FindSolutionFile(string rootDirectory)
        {
            if (!Directory.Exists(rootDirectory))
            {
              throw new Exception($"No such directory: '{rootDirectory}'");
            }

            return FindSolutionFileInDirRecursive(rootDirectory);
        }

        private string FindSolutionFileInDirRecursive(string dir)
        {
            var current = ScanForSolutionFile(dir);
            if (!string.IsNullOrWhiteSpace(current))
            {
                return current;
            }

            var subdirs = Directory.EnumerateDirectories(dir);
                
            foreach (var subdir in subdirs)
            {
                var subdirSln = FindSolutionFileInDirRecursive(subdir);
                if (!string.IsNullOrWhiteSpace(subdirSln))
                {
                    return subdirSln;
                }
            }

            return string.Empty;
        }

        private string ScanForSolutionFile(string dir)
        {
            var files = Directory.EnumerateFiles(dir, "*.sln");
            var slnFile = files.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(slnFile))
            {
                return string.Empty;
            }

            return Path.Combine(dir, slnFile);
        }
    }
}