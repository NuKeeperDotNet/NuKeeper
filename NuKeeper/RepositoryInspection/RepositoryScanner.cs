using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NuKeeper.RepositoryInspection
{
    public class RepositoryScanner: IRepositoryScanner
    {
        public IEnumerable<NuGetPackage> FindAllNuGetPackages(string rootDirectory)
        {
            if (!Directory.Exists(rootDirectory))
            {
              throw new Exception($"No such directory: '{rootDirectory}'");
            }

            return FindNuGetPackagesInDirRecursive(rootDirectory);
        }

        private IEnumerable<NuGetPackage> FindNuGetPackagesInDirRecursive(string dir)
        {
            var current = ScanForNuGetPackages(dir);

            var subDirs = Directory.EnumerateDirectories(dir);
                
            foreach (var subDir in subDirs)
            {
                // subDir is a full path, check the last part
                var dirName = new DirectoryInfo(subDir).Name;
                if (!IsExcluded(dirName))
                {
                    var subdirPackages = FindNuGetPackagesInDirRecursive(subDir);
                    current.AddRange(subdirPackages);
                }
            }

            return current;
        }

        private readonly List<string> _excludedDirNames = new List<string>
        {
            ".git",
            ".vs",
            "obj",
            "bin",
            "node_modules",
            "packages"
        };

        private bool IsExcluded(string dirName)
        {
            return _excludedDirNames.Any(s => string.Equals(s, dirName, StringComparison.OrdinalIgnoreCase));
        }

        private List<NuGetPackage> ScanForNuGetPackages(string dir)
        {
            var result = new List<NuGetPackage>();
            var files = Directory.EnumerateFiles(dir);

            foreach (var fileName in files)
            {
                var fileNameWithoutPath = Path.GetFileName(fileName);
                if (string.Equals(fileNameWithoutPath, "packages.config"))
                {
                    var packages = PackagesFileReader.ReadFile(fileName);
                    SetSourceFilePath(packages, fileName);
                    result.AddRange(packages);
                }
                else if (fileName.EndsWith(".csproj"))
                {
                    var packages = ProjectFileReader.ReadFile(fileName);
                    SetSourceFilePath(packages, fileName);
                    result.AddRange(packages);
                }
            }

            return result;
        }

        private void SetSourceFilePath(IEnumerable<NuGetPackage> packages, string sourceFilePath)
        {
            foreach (var package in packages)
            {
                package.SourceFilePath = sourceFilePath;
            }
        }
    }
}