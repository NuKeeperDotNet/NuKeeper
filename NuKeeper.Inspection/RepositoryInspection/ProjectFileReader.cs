using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NuKeeper.Types.Logging;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public class ProjectFileReader
    {
        private const string VisualStudioLegacyProjectNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        private readonly INuKeeperLogger _logger;

        public ProjectFileReader(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<PackageInProject> ReadFile(string baseDirectory, string relativePath)
        {
            using (var fileContents = File.OpenRead(Path.Combine(baseDirectory, relativePath)))
            {
                return Read(fileContents, baseDirectory, relativePath);
            }
        }

        public IEnumerable<PackageInProject> Read(Stream fileContents, string baseDirectory, string relativePath)
        {
            var xml = XDocument.Load(fileContents);
            var ns = xml.Root.GetDefaultNamespace();

            var path = CreatePackagePath(ns, baseDirectory, relativePath);

            var project = xml.Element(ns + "Project");

            if (project == null)
            {
                return Enumerable.Empty<PackageInProject>();
            }

            var itemGroups = project.Elements(ns + "ItemGroup");
            var packageRefs = itemGroups.SelectMany(ig => ig.Elements(ns + "PackageReference"));

            return packageRefs
                .Select(el => XmlToPackage(el, path))
                .Where(el => el != null)
                .ToList();
        }

        private static PackagePath CreatePackagePath(XNamespace xmlNamespace, string baseDirectory, string relativePath)
        {
            return xmlNamespace.NamespaceName == VisualStudioLegacyProjectNamespace
                ? new PackagePath(baseDirectory, relativePath, PackageReferenceType.ProjectFileOldStyle)
                : new PackagePath(baseDirectory, relativePath, PackageReferenceType.ProjectFile);
        }

        private PackageInProject XmlToPackage(XElement el, PackagePath path)
        {
            try
            {
                var id = el.Attribute("Include")?.Value;
                var version = el.Attribute("Version")?.Value ?? el.Value;

                return new PackageInProject(id, version, path);
            }
            catch (Exception ex)
            {
                _logger.Error($"Could not read package from {el} in file {path.FullName}", ex);
                return null;
            }
        }
    }
}
