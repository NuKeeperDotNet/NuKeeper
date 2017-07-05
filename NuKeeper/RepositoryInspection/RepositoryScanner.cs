using System;
using System.Collections.Generic;
using System.IO;
using NuKeeper.Nuget;

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
            var current = FindNugetPackagesInDir(dir);

            var subdirs = Directory.EnumerateDirectories(dir);
                
            foreach (var subdir in subdirs)
            {
                var subdirPackages = FindNugetPackagesInDirRecursive(subdir);
                current.AddRange(subdirPackages);
            }

            return current;
        }

        private List<NugetPackage> FindNugetPackagesInDir(string dir)
        {
            var result = new List<NugetPackage>();
            var files = Directory.EnumerateFiles(dir);

            foreach (var fileName in files)
            {
                if (string.Equals(fileName, "packages.config"))
                {
                    var fullName = Path.Combine(dir, fileName);
                    var packages = PackagesFileReader.Read(fullName);
                    result.AddRange(packages);
                }
                else if (fileName.EndsWith(".csproj"))
                {
                    var fullName = Path.Combine(dir, fileName);
                    var packages = ProjectFileReader.Read(fullName);
                    result.AddRange(packages);
                }
            }

            return result;
        }
    }
}