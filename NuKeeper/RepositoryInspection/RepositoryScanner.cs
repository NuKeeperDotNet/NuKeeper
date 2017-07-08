using System;
using System.Collections.Generic;
using System.IO;

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

            return FindNugetPackagesInDirRecursive(rootDirectory, rootDirectory);
        }

        private IEnumerable<NuGetPackage> FindNugetPackagesInDirRecursive(string rootDir, string dir)
        {
            var current = ScanForNugetPackages(rootDir, dir);

            var subDirs = Directory.EnumerateDirectories(dir);
                
            foreach (var subDir in subDirs)
            {
                if (!DirectoryExclusions.PathIsExcluded(subDir))
                {
                    var subdirPackages = FindNugetPackagesInDirRecursive(rootDir, subDir);
                    current.AddRange(subdirPackages);
                }
            }

            return current;
        }

        private List<NuGetPackage> ScanForNugetPackages(string rootDir, string dir)
        {
            var result = new List<NuGetPackage>();
            var files = Directory.EnumerateFiles(dir);

            foreach (var fileName in files)
            {
                var fileNameWithoutPath = Path.GetFileName(fileName);

                if (string.Equals(fileNameWithoutPath, "packages.config"))
                {
                    var path = MakePackagePath(rootDir, fileName);
                    var packages = PackagesFileReader.ReadFile(path);
                    result.AddRange(packages);
                }
                else if (fileName.EndsWith(".csproj"))
                {
                    var path = MakePackagePath(rootDir, fileName);
                    var packages = ProjectFileReader.ReadFile(path);
                    result.AddRange(packages);
                }
            }

            return result;
        }

        private PackagePath MakePackagePath(string rootDir, string fileName)
        {
            var relativeFileName = fileName.Replace(rootDir, string.Empty);
            return new PackagePath(rootDir, relativeFileName);
        }
    }
}