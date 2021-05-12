using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Inspection.Sort;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public class RepositoryScanner : IRepositoryScanner
    {
        private readonly INuKeeperLogger _logger;
        private readonly IReadOnlyCollection<IPackageReferenceFinder> _finders;
        private readonly IDirectoryExclusions _directoryExclusions;

        private ProjectFileReader _projectFileReader;

        public RepositoryScanner(
            INuKeeperLogger logger,
            ProjectFileReader projectFileReader,
            PackagesFileReader packagesFileReader,
            NuspecFileReader nuspecFileReader,
            DirectoryBuildTargetsReader directoryBuildTargetsReader,
            IDirectoryExclusions directoryExclusions)
        {
            _logger = logger;

            _finders = new IPackageReferenceFinder[]
            {
                projectFileReader, packagesFileReader, nuspecFileReader, directoryBuildTargetsReader
            };

            _directoryExclusions = directoryExclusions;

            _projectFileReader = projectFileReader;
        }

        public IReadOnlyCollection<PackageInProject> FindAllNuGetPackages(IFolder workingFolder)
        {
            var packages = _finders
                .SelectMany(f => FindPackages(workingFolder, f))
                .ToList();

            var projectSorter = new ProjectReferenceTopologicalSort(_logger);

            var sortedProjects = projectSorter.Sort(BuildReferencesToAllRelevantPackageSources(packages))
                .ToList();

            sortedProjects.Reverse();

            var packagesInProjects = GetPackagesByReferenceSource(packages);

            return sortedProjects.Where(project => packagesInProjects.ContainsKey(project)).SelectMany(project => packagesInProjects[project]).ToList();
        }

        private ReadOnlyDictionary<string, IReadOnlyCollection<string>> BuildReferencesToAllRelevantPackageSources(List<PackageInProject> packages)
        {
            var references = _projectFileReader.ProjectReferences.ToDictionary(kvp => kvp.Key, kvp => new HashSet<string>(kvp.Value));

            var packagesByType = GroupPackagesByReferenceType(packages);

            foreach (var packageReferenceTypeCollection in packagesByType)
            {
                var packageReferencesForCurrentType = packageReferenceTypeCollection.Value;

                AddEmptyReferenceForMissingFiles(references, packageReferencesForCurrentType);
            }

            foreach (var packageReferenceTypeCollection in packagesByType)
            {
                var packageReferenceType = packageReferenceTypeCollection.Key;
                var packageReferencesForCurrentType = packageReferenceTypeCollection.Value;

                switch (packageReferenceType)
                {
                    case PackageReferenceType.DirectoryBuildTargets:
                        AddReferencesForDirectoryBuildProps(references, packageReferencesForCurrentType);
                        break;
                    case PackageReferenceType.PackagesConfig:
                        AddReferencesForPackagesConfig(references, packageReferencesForCurrentType);
                        break;
                    default:
                        break;
                }
            }

            return new ReadOnlyDictionary<string, IReadOnlyCollection<string>>(references.ToDictionary(project => project.Key, project => (IReadOnlyCollection<string>)project.Value));
        }

        private static void AddEmptyReferenceForMissingFiles(Dictionary<string, HashSet<string>> references, IEnumerable<string> packageReferenceFiles)
        {
            foreach (var projectFile in packageReferenceFiles)
            {
                if (!references.ContainsKey(projectFile))
                {
                    references.Add(projectFile, new());
                }
            }
        }

        private void AddReferencesForPackagesConfig(Dictionary<string, HashSet<string>> references, IEnumerable<string> packagesConfigFiles)
        {
            foreach (var packagesConfigFile in packagesConfigFiles)
            {
                var currentDirectory = Path.GetDirectoryName(packagesConfigFile);
                var projectsInCurrentDirectory = references.Where(projectReference => Path.GetDirectoryName(projectReference.Key) == currentDirectory && _projectFileReader.GetFilePatterns().Any(pattern => projectReference.Key.EndsWith(pattern, StringComparison.InvariantCultureIgnoreCase)));

                foreach (var projectInCurrentDirectory in projectsInCurrentDirectory)
                {
                    references[projectInCurrentDirectory.Key].Add(packagesConfigFile);
                }
            }
        }

        private static void AddReferencesForDirectoryBuildProps(Dictionary<string, HashSet<string>> projectReferences, IEnumerable<string> directoryBuildPropsFiles)
        {
            foreach (var directoryBuildPropsFile in directoryBuildPropsFiles)
            {
                var path = Path.GetDirectoryName(directoryBuildPropsFile);
                foreach (var project in projectReferences)
                {
                    if (project.Key != directoryBuildPropsFile && project.Key.Contains(path))
                    {
                        projectReferences[project.Key].Add(directoryBuildPropsFile);
                    }
                }
            }
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
                        f.FullName))).ToList();
        }

        private static Dictionary<PackageReferenceType, HashSet<string>> GroupPackagesByReferenceType(List<PackageInProject> packages)
        {
            var packagesByReferenceType = new Dictionary<PackageReferenceType, HashSet<string>>();

            foreach (var package in packages)
            {
                if (!packagesByReferenceType.ContainsKey(package.Path.PackageReferenceType))
                    packagesByReferenceType.Add(package.Path.PackageReferenceType, new());

                var packagesOfCurrentReferenceType = packagesByReferenceType[package.Path.PackageReferenceType];

                packagesOfCurrentReferenceType.Add(package.Path.FullName);
            }

            return packagesByReferenceType;
        }

        private static Dictionary<string, List<PackageInProject>> GetPackagesByReferenceSource(List<PackageInProject> packages)
        {
            var packagesProjectsInfos = new Dictionary<string, List<PackageInProject>>();

            foreach (var package in packages)
            {
                var referencingProject = package.Path.FullName;
                if (!packagesProjectsInfos.ContainsKey(referencingProject))
                    packagesProjectsInfos.Add(referencingProject, new());

                packagesProjectsInfos[referencingProject].Add(package);
            }

            return packagesProjectsInfos;
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
