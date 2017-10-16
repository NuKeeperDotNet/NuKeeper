using System.Collections.Generic;
using System.Linq;
using NuKeeper.Files;

namespace NuKeeper.RepositoryInspection
{
    public class RepositoryScanner: IRepositoryScanner
    {
        private readonly ProjectFileReader _projectFileReader;
        private readonly PackagesFileReader _packagesFileReader;

        public RepositoryScanner(ProjectFileReader projectFileReader, PackagesFileReader packagesFileReader)
        {
            _projectFileReader = projectFileReader;
            _packagesFileReader = packagesFileReader;
        }

        public IEnumerable<PackageInProject> FindAllNuGetPackages(IFolder workingFolder)
        {

            return 
                PackageFiles(workingFolder)
                .Concat(ProjectFiles(workingFolder))
                .ToList();
        }

        private IEnumerable<PackageInProject> PackageFiles(IFolder workingFolder)
        {
            var packagesFiles = workingFolder.Find("packages.config")
                .Where(f => !DirectoryExclusions.PathIsExcluded(f.FullName));

            var results = new List<PackageInProject>();

            foreach (var packagesFile in packagesFiles)
            {
                var path = MakePackagePath(workingFolder.FullPath, packagesFile.FullName, PackageReferenceType.PackagesConfig);
                var packages = _packagesFileReader.ReadFile(path);
                results.AddRange(packages);
            }

            return results;
        }

        private IEnumerable<PackageInProject> ProjectFiles(IFolder workingFolder)
        {
            var projectFiles = workingFolder.Find("*.csproj")
                .Concat(workingFolder.Find("*.vbproj"))
                .Where(f => !DirectoryExclusions.PathIsExcluded(f.FullName));

            var results = new List<PackageInProject>();

            foreach (var projectFile in projectFiles)
            {
                var path = MakePackagePath(workingFolder.FullPath, projectFile.FullName, PackageReferenceType.ProjectFile);
                var packages = _projectFileReader.ReadFile(path);
                results.AddRange(packages);
            }

            return results;
        }

        private PackagePath MakePackagePath(string rootDir, string fileName, 
            PackageReferenceType packageReferenceType)
        {
            var relativeFileName = fileName.Replace(rootDir, string.Empty);
            return new PackagePath(rootDir, relativeFileName, packageReferenceType);
        }
    }
}