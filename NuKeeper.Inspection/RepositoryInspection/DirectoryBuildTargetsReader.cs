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
    public class DirectoryBuildTargetsReader : IPackageReferenceFinder
    {
        private readonly INuKeeperLogger _logger;
        private readonly PackageInProjectReader _packageInProjectReader;

        public DirectoryBuildTargetsReader(INuKeeperLogger logger)
        {
            _logger = logger;
            _packageInProjectReader = new PackageInProjectReader(logger);
        }

        public IReadOnlyCollection<PackageInProject> ReadFile(string baseDirectory, string relativePath)
        {
            var packagePath = new PackagePath(baseDirectory, relativePath, PackageReferenceType.DirectoryBuildTargets);
            try
            {
                using (var fileContents = File.OpenRead(packagePath.FullName))
                {
                    return Read(fileContents, packagePath);
                }
            }
            catch (IOException ex)
            {
                throw new NuKeeperException($"Unable to parse file {packagePath.FullName}", ex);
            }
        }

        public IReadOnlyCollection<string> GetFilePatterns()
        {
            return new[] { "Directory.Build.props", "Directory.Packages.props", "Directory.Build.targets", "Packages.props" };
        }

        public IReadOnlyCollection<PackageInProject> Read(Stream fileContents, PackagePath path)
        {
            var xml = XDocument.Load(fileContents);

            var packagesNode = xml.Element("Project")?.Elements("ItemGroup");
            if (packagesNode == null)
            {
                return Array.Empty<PackageInProject>();
            }

            var packageRefs = packagesNode.Elements("PackageReference");
            var packageDownloads = packagesNode.Elements("PackageDownload");
            var packageVersions = packagesNode.Elements("PackageVersion");

            return packageRefs
                .Concat(packageDownloads)
                .Concat(packageVersions)
                .Select(el => XmlToPackage(el, path))
                .Where(el => el != null)
                .ToList();
        }

        private PackageInProject XmlToPackage(XElement el, PackagePath path)
        {
            var id = el.Attribute("Include")?.Value;
            if (id == null)
            {
                id = el.Attribute("Update")?.Value;
            }
            var version = el.Name == "PackageDownload"
                ? GetVersion(el)?.Trim('[', ']')
                : GetVersion(el);

            return _packageInProjectReader.Read(id, version, path, null);
        }

        private static string GetVersion(XElement el, XNamespace ns = null)
        {
            return ns == null
                ? el.Attribute("Version")?.Value ?? el.Element("Version")?.Value
                : el.Attribute("Version")?.Value ?? el.Element(ns + "Version")?.Value;
        }
    }
}
