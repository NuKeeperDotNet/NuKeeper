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

            var importNodes = project.Elements("Import");
            foreach (var import in importNodes)
            {
                var projectAttribute = import.Attribute("Project");
                if (projectAttribute == null) continue;
                var importPath = projectAttribute.Value.Replace("$(MSBuildThisFileDirectory)", "").Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                try
                {
                    results.AddRange(ReadFile(path.BaseDirectory, importPath));
                }
                catch (NuKeeperException)
                {
                    _logger.Detailed($"Unable to handle path for importPath {importPath}");
                }
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
            var version = el.Attribute("Version")?.Value;

            return _packageInProjectReader.Read(id, version, path, null);
        }
    }
}
