using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NuKeeper.Abstractions.Inspections.Files;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public class RepositoryScanner : IRepositoryScanner
    {
        private readonly IReadOnlyCollection<IPackageReferenceFinder> _finders;
        private readonly IDirectoryExclusions _directoryExclusions;

        public RepositoryScanner(
            ProjectFileReader projectFileReader,
            PackagesFileReader packagesFileReader,
            NuspecFileReader nuspecFileReader,
            DirectoryBuildTargetsReader directoryBuildTargetsReader,
            IDirectoryExclusions directoryExclusions)
        {
            _finders = new IPackageReferenceFinder[]
            {
                projectFileReader, packagesFileReader, nuspecFileReader, directoryBuildTargetsReader
            };

            _directoryExclusions = directoryExclusions;
        }

        public IReadOnlyCollection<PackageInProject> FindAllNuGetPackages(IFolder workingFolder)
        {
            return _finders
                .SelectMany(f => FindPackages(workingFolder, f))
                .ToList();
        }

        private IEnumerable<PackageInProject> FindPackages(IFolder workingFolder,
            IPackageReferenceFinder packageReferenceFinder)
        {
            var files = packageReferenceFinder
                .GetFilePatterns()
                .SelectMany(workingFolder.Find);

            var filesInUsableDirectories =
                files.Where(f => !_directoryExclusions.PathIsExcluded(f.FullName));

            return filesInUsableDirectories.SelectMany(f =>
                packageReferenceFinder.ReadFile(
                    workingFolder.FullPath,
                    GetRelativeFileName(
                        workingFolder.FullPath,
                        f.FullName)));
        }

        private static string GetRelativeFileName(string rootDir, string fileName)
        {
            var separatorChar = Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture);
            var rootDirWithSeparator = rootDir.EndsWith(separatorChar, StringComparison.OrdinalIgnoreCase)
                ? rootDir
                : rootDir + Path.DirectorySeparatorChar;
            return fileName.Replace(rootDirWithSeparator, string.Empty);
        }
    }
}
