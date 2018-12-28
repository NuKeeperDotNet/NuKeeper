using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NuGet.Packaging.Core;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;

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
            catch (Exception ex)
            {
                throw new ApplicationException($"Unable to parse file {filePath}", ex);
            }
        }

        public IReadOnlyCollection<string> GetFilePatterns()
        {
            return new[] {"*.csproj", "*.vbproj", "*.fsproj" };
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

            var itemGroups = project
                .Elements(ns + "ItemGroup")
                .ToList();

            var projectRefs = itemGroups
                .SelectMany(ig => ig.Elements(ns + "ProjectReference"))
                .Select(el => MakeProjectPath(el, path.FullName))
                .ToList();

            var packageRefs = itemGroups.SelectMany(ig => ig.Elements(ns + "PackageReference"));

            return packageRefs
                .Select(el => XmlToPackage(ns, el, path, projectRefs))
                .Where(el => el != null)
                .ToList();
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
            try
            {
                var id = el.Attribute("Include")?.Value;
                var version = el.Attribute("Version")?.Value ?? el.Element(ns + "Version")?.Value;

                if (string.IsNullOrWhiteSpace(version))
                {
                    _logger.Normal($"Skipping package '{id}' with no version specified.");
                    return null;
                }

                var packageVersionRange = PackageVersionRange.Read(id, version);

                if (packageVersionRange == null)
                {
                    _logger.Normal($"Skipping package '{id}' with version '{version}' that could not be parsed.");
                    return null;
                }

                var singleVersion = packageVersionRange.SingleVersionIdentity();

                if (singleVersion == null)
                {
                    _logger.Normal($"Skipping package '{id}' with version range '{version}' that is not a single version.");
                    return null;
                }

                return new PackageInProject(singleVersion, path, projectReferences);
            }
            catch (Exception ex)
            {
                _logger.Error($"Could not read package from {el} in file {path.FullName}", ex);
                return null;
            }
        }
    }
}
