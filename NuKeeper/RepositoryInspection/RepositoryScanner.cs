using System.Collections.Generic;
using System.IO;
using NuKeeper.Files;

namespace NuKeeper.RepositoryInspection
{
    public class RepositoryScanner: IRepositoryScanner
    {
        public IEnumerable<PackageInProject> FindAllNuGetPackages(IFolder workingFolder)
        {
            return FindNugetPackagesInDirRecursive(workingFolder.FullPath, workingFolder.FullPath);
        }

        private IEnumerable<PackageInProject> FindNugetPackagesInDirRecursive(string rootDir, string dir)
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

        private List<PackageInProject> ScanForNugetPackages(string rootDir, string dir)
        {
            var result = new List<PackageInProject>();

            var packagesConfigPath = Path.Combine(dir, "packages.config");

            if (File.Exists(packagesConfigPath))
            {
                var path = MakePackagePath(rootDir, packagesConfigPath, PackageReferenceType.PackagesConfig);
                var packages = PackagesFileReader.ReadFile(path);
                result.AddRange(packages);
            }
            else
            {
                var files = Directory.GetFiles(dir, "*.csproj", SearchOption.TopDirectoryOnly);

                foreach (var fileName in files)
                {
                    var path = MakePackagePath(rootDir, fileName, PackageReferenceType.ProjectFile);
                    var packages = ProjectFileReader.ReadFile(path);
                    result.AddRange(packages);
                }
            }

            return result;
        }

        private PackagePath MakePackagePath(string rootDir, string fileName, 
            PackageReferenceType packageReferenceType)
        {
            var relativeFileName = fileName.Replace(rootDir, string.Empty);
            return new PackagePath(rootDir, relativeFileName, packageReferenceType);
        }
    }
}