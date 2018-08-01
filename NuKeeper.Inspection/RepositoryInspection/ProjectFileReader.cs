using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public class ProjectFileReader : IPackageReferenceFinder
    {
        private const string VisualStudioLegacyProjectNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        private readonly INuKeeperLogger _logger;

        public ProjectFileReader(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<PackageInProject> ReadFile(string baseDirectory, string relativePath)
        {
            var filePath = Path.Combine(baseDirectory, relativePath);
            try
            {
                using (var fileContents = File.OpenRead(filePath))
                {
                    return Read(fileContents, baseDirectory, relativePath);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Unable to parse file {filePath}", ex);
            }
        }

        public IEnumerable<string> GetFilePatterns()
        {
            return new[] {"*.csproj", "*.vbproj", "*.fsproj"};
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
                .Select(el => XmlToPackage(el, path, ns))
                .Where(el => el != null)
                .ToList();
        }

        private static PackagePath CreatePackagePath(XNamespace xmlNamespace, string baseDirectory, string relativePath)
        {
            return xmlNamespace.NamespaceName == VisualStudioLegacyProjectNamespace
                ? new PackagePath(baseDirectory, relativePath, PackageReferenceType.ProjectFileOldStyle)
                : new PackagePath(baseDirectory, relativePath, PackageReferenceType.ProjectFile);
        }

        private PackageInProject XmlToPackage(XElement el, PackagePath path, XNamespace ns)
        {
            try
            {
                var id = el.Attribute("Include")?.Value;
                var version = el.Attribute("Version")?.Value ?? el.Element(ns + "Version")?.Value;

                if (string.IsNullOrWhiteSpace(version))
                {
                    _logger.Normal($"Skipping package '{id}' with no version specified.");
                    return null;
                }

                var versionParseSuccess = NuGetVersion.TryParse(version, out var nugetVersion);
                if (!versionParseSuccess)
                {
                    _logger.Normal($"Skipping package '{id}' with version '{version}' that could not be parsed.");
                    return null;
                }

                return new PackageInProject(new PackageIdentity(id, nugetVersion), path);
            }
            catch (Exception ex)
            {
                _logger.Error($"Could not read package from {el} in file {path.FullName}", ex);
                return null;
            }
        }
    }
}
