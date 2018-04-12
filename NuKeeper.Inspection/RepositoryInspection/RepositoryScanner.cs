using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuKeeper.Inspection.Files;

namespace NuKeeper.Inspection.RepositoryInspection
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

            return PackageFiles(workingFolder)
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
                var packages = _packagesFileReader.ReadFile(workingFolder.FullPath,
                    GetRelativeFileName(workingFolder.FullPath, packagesFile.FullName));
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
                var packages = _projectFileReader.ReadFile(workingFolder.FullPath,
                    GetRelativeFileName(workingFolder.FullPath, projectFile.FullName));
                results.AddRange(packages);
            }

            return results;
        }

        private string GetRelativeFileName(string rootDir, string fileName)
        {
            var rootDirWithSeparator = rootDir.EndsWith(Path.DirectorySeparatorChar.ToString())
                ? rootDir
                : rootDir + Path.DirectorySeparatorChar;
            return fileName.Replace(rootDirWithSeparator, string.Empty);
        }
    }
}
