using System;
using System.Collections.Generic;
using System.IO;

namespace NuKeeper.RepositoryInspection
{
    public class RepositoryScanner: IRepositoryScanner
    {
        public IEnumerable<NugetPackage> FindAllNugetPackages(string rootDirectory)
        {
            if (!Directory.Exists(rootDirectory))
            {
              throw new Exception($"No such directory: '{rootDirectory}'");
            }

            return FindNugetPackagesInDirRecursive(rootDirectory);
        }

        private IEnumerable<NugetPackage> FindNugetPackagesInDirRecursive(string dir)
        {
            var current = ScanForNugetPackages(dir);

            var subdirs = Directory.EnumerateDirectories(dir);
                
            foreach (var subdir in subdirs)
            {
                var subdirPackages = FindNugetPackagesInDirRecursive(subdir);
                current.AddRange(subdirPackages);
            }

            return current;
        }

        private List<NugetPackage> ScanForNugetPackages(string dir)
        {
            var result = new List<NugetPackage>();
            var files = Directory.EnumerateFiles(dir);

            foreach (var fileName in files)
            {
                if (string.Equals(fileName, "packages.config"))
                {
                    var packagesFullPath = Path.Combine(dir, fileName);
                    var packages = PackagesFileReader.ReadFile(packagesFullPath);
                    SetSourceFilePath(packages, packagesFullPath);
                    result.AddRange(packages);
                }
                else if (fileName.EndsWith(".csproj"))
                {
                    var csprojFullPath = Path.Combine(dir, fileName);
                    var packages = ProjectFileReader.ReadFile(csprojFullPath);
                    SetSourceFilePath(packages, csprojFullPath);
                    result.AddRange(packages);
                }
            }

            return result;
        }

        private void SetSourceFilePath(IEnumerable<NugetPackage> packages, string sourceFilePath)
        {
            foreach (var package in packages)
            {
                package.SourceFilePath = sourceFilePath;
            }
        }
    }
}