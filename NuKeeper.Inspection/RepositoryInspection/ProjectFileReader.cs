using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public class ProjectFileReader : IPackageReferenceFinder
    {
        private const string VisualStudioLegacyProjectNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        private readonly INuKeeperLogger _logger;
        private readonly PackageInProjectReader _packageInProjectReader;

        public ProjectFileReader(INuKeeperLogger logger)
        {
            _logger = logger;
            _packageInProjectReader = new PackageInProjectReader(logger);
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
            var xml = XDocument.Load(fileContents);
            var ns = xml.Root.GetDefaultNamespace();

            var path = CreatePackagePath(ns, baseDirectory, relativePath);

            var project = xml.Element(ns + "Project");

            if (project == null)
            {
                return Array.Empty<PackageInProject>();
            }

            var projectFileResults = new List<PackageInProject>();

            var itemGroups = project
                .Elements(ns + "ItemGroup")
                .ToList();

            var projectRefs = itemGroups
                .SelectMany(ig => ig.Elements(ns + "ProjectReference"))
                .Select(el => MakeProjectPath(el, path.FullName))
                .ToList();

            var packageRefs = itemGroups.SelectMany(ig => ig.Elements(ns + "PackageReference"));
            projectFileResults.AddRange(
                packageRefs
                .Select(el => XmlToPackage(ns, el, path, projectRefs))
                .Where(el => el != null)
            );

            projectFileResults.AddRange(
                itemGroups
                    .SelectMany(ig => ig.Elements(ns + "PackageDownloads"))
                    .Select(el => XmlToPackage(ns, el, new PackagePath(baseDirectory, relativePath, PackageReferenceType.DirectoryBuildTargets), null))
                    .Where(el => el != null)
            );

            projectFileResults.AddRange(
                itemGroups
                    .SelectMany(ig => ig.Elements(ns + "PackageVersion"))
                    .Select(el => XmlToPackage(ns, el, new PackagePath(baseDirectory, relativePath, PackageReferenceType.DirectoryBuildTargets), null))
                    .Where(el => el != null)
            );

            return projectFileResults;
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
            var version = el.Name == "PackageDownload"
                ? GetVersion(el, ns)?.Trim('[', ']')
                : GetVersion(el, ns);

            return _packageInProjectReader.Read(id, version, path, projectReferences);
        }

        private static string GetVersion(XElement el, XNamespace ns = null)
        {
            return ns == null
                ? el.Attribute("Version")?.Value ?? el.Element("Version")?.Value
                : el.Attribute("Version")?.Value ?? el.Element(ns + "Version")?.Value;
        }
    }
}
