using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public class ProjectFileReader : IPackageReferenceFinder
    {
        private const string VisualStudioLegacyProjectNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        private readonly INuKeeperLogger _logger;
        private readonly PackageInProjectReader _packageInProjectReader;
        private readonly DirectoryBuildTargetsReader _directoryBuildTargetsReader;

        public ProjectFileReader(INuKeeperLogger logger)
        {
            _logger = logger;
            _packageInProjectReader = new PackageInProjectReader(logger);
            _directoryBuildTargetsReader = new DirectoryBuildTargetsReader(logger);
        }

        public IReadOnlyCollection<PackageInProject> ReadFile(string baseDirectory, string relativePath)
        {
            var filePath = Path.Combine(baseDirectory, relativePath);
            try
            {
                using (var fileContents = File.OpenRead(filePath))
                {
                    return Read(fileContents, baseDirectory, relativePath);
                }
            }
            catch (IOException ex)
            {
                throw new ApplicationException($"Unable to parse file {filePath}", ex);
            }
        }

        public IReadOnlyCollection<string> GetFilePatterns()
        {
            return new[] { "*.csproj", "*.vbproj", "*.fsproj" };
        }

        public IReadOnlyCollection<PackageInProject> Read(Stream fileContents, string baseDirectory, string relativePath)
        {
            var results = new List<PackageInProject>();
            var xml = XDocument.Load(fileContents);
            var ns = xml.Root.GetDefaultNamespace();

            var path = CreatePackagePath(ns, baseDirectory, relativePath);

            var project = xml.Element(ns + "Project");

            if (project == null)
            {
                return Array.Empty<PackageInProject>();
            }

            var imports = MsBuildExtensions.ExtractImports(project, Path.GetDirectoryName(path.RelativePath));
            foreach (var importPath in imports)
            {
                try
                {
                    results.AddRange(_directoryBuildTargetsReader.ReadFile(path.BaseDirectory, importPath));
                }
                catch (NuKeeperException)
                {
                    _logger.Detailed($"Unable to handle path for importPath {importPath} with base directory {baseDirectory} {relativePath}");
                }
            }

            var itemGroups = project
                .Elements(ns + "ItemGroup")
                .ToList();

            var projectRefs = itemGroups
                .SelectMany(ig => ig.Elements(ns + "ProjectReference"))
                .Select(el => MakeProjectPath(el, path.FullName))
                .ToList();

            var packageRefs = itemGroups.SelectMany(ig => ig.Elements(ns + "PackageReference"));
            var globalPackageRefs = itemGroups.SelectMany(ig => ig.Elements(ns + "GlobalPackageReference"));

            results.AddRange(packageRefs
                .Select(el => XmlToPackage(ns, el, path, projectRefs))
                .Where(el => el != null));

            foreach (var globalPackageref in globalPackageRefs)
            {
                var refPath = new PackagePath(baseDirectory, relativePath, PackageReferenceType.DirectoryBuildTargets);
                results.Add(XmlToPackage(ns, globalPackageref, refPath, null));
            }

            var sdkNodes = project.Elements("Sdk");
            foreach (var import in sdkNodes)
            {
                var nameAttribute = import.Attribute("Name");
                if (nameAttribute == null) continue;
                var versionAttribute = import.Attribute("Version");
                if (versionAttribute == null) continue;

                var refPath = new PackagePath(baseDirectory, relativePath, PackageReferenceType.DirectoryBuildTargets);
                results.Add(XmlToSdk(ns, import, refPath));
            }

            return results;
        }

        private static string MakeProjectPath(XElement el, string currentPath)
        {
            var relativePath = el.Attribute("Include")?.Value;

            var currentDir = Path.GetDirectoryName(currentPath);
            var combinedPath = Path.Combine(currentDir, relativePath);

            // combined path can still have "\..\" parts to it, need to canonicalise
            return Path.GetFullPath(combinedPath);
        }

        private static PackagePath CreatePackagePath(XNamespace xmlNamespace, string baseDirectory, string relativePath)
        {
            return xmlNamespace.NamespaceName == VisualStudioLegacyProjectNamespace
                ? new PackagePath(baseDirectory, relativePath, PackageReferenceType.ProjectFileOldStyle)
                : new PackagePath(baseDirectory, relativePath, PackageReferenceType.ProjectFile);
        }

        private PackageInProject XmlToPackage(XNamespace ns, XElement el,
            PackagePath path, IEnumerable<string> projectReferences)
        {
            var id = el.Attribute("Include")?.Value;
            var version = el.Attribute("Version")?.Value ?? el.Element(ns + "Version")?.Value;

            return _packageInProjectReader.Read(id, version, path, projectReferences);
        }

        private PackageInProject XmlToSdk(XNamespace ns, XElement el, PackagePath path)
        {
            var id = el.Attribute("Name")?.Value;
            var version = el.Attribute("Version")?.Value ?? el.Element(ns + "Version")?.Value;

            return _packageInProjectReader.Read(id, version, path, null);
        }
    }
}
