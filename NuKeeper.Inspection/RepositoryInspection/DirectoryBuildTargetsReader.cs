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
            return new[] { "Directory.Build.props", "Directory.Build.targets", "Packages.props" };
        }

        public IReadOnlyCollection<PackageInProject> Read(Stream fileContents, PackagePath path)
        {
            var results = new List<PackageInProject>();
            var xml = XDocument.Load(fileContents);

            var project = xml.Element("Project");
            if (project == null)
            {
                return Array.Empty<PackageInProject>();
            }

            var imports = MsBuildExtensions.ExtractImports(project, Path.GetDirectoryName(path.RelativePath));
            foreach (var importPath in imports)
            {
                try
                {
                    results.AddRange(ReadFile(path.BaseDirectory, importPath));
                }
                catch (NuKeeperException)
                {
                    _logger.Detailed($"Unable to handle path for importPath {importPath} with base directory {path.BaseDirectory} {path.RelativePath}");
                }
            }

            var sdkNodes = project.Elements("Sdk");
            foreach (var import in sdkNodes)
            {
                var nameAttribute = import.Attribute("Name");
                if (nameAttribute == null) continue;
                var versionAttribute = import.Attribute("Version");
                if (versionAttribute == null) continue;

                results.Add(XmlToSdk(import, path));
            }

            var itemGroupNodes = project.Elements("ItemGroup");
            var packageNodeList = itemGroupNodes.Elements("PackageReference")
                .Concat(itemGroupNodes.Elements("GlobalPackageReference"));

            results.AddRange(packageNodeList
                .Select(el => XmlToPackage(el, path))
                .Where(el => el != null));

            return results;
        }

        private PackageInProject XmlToPackage(XElement el, PackagePath path)
        {
            var id = el.Attribute("Include")?.Value;
            if (id == null)
            {
                id = el.Attribute("Update")?.Value;
            }
            var version = el.Attribute("Version")?.Value ?? el.Element("Version")?.Value;

            return _packageInProjectReader.Read(id, version, path, null);
        }

        private PackageInProject XmlToSdk(XElement el, PackagePath path)
        {
            var id = el.Attribute("Name")?.Value;
            var version = el.Attribute("Version")?.Value ?? el.Element("Version")?.Value;

            return _packageInProjectReader.Read(id, version, path, null);
        }
    }
}
