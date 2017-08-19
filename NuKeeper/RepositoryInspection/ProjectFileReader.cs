using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NuKeeper.Logging;

namespace NuKeeper.RepositoryInspection
{
    public class ProjectFileReader
    {
        private readonly INuKeeperLogger _logger;

        public ProjectFileReader(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<PackageInProject> ReadFile(PackagePath path)
        {
            var fileContents = File.ReadAllText(path.FullPath);
            return Read(fileContents, path);
        }

        private IEnumerable<PackageInProject> Read(string fileContents, PackagePath path)
        {
            var xml = XDocument.Parse(fileContents);
            var project = xml.Element("Project");

            if (project == null)
            {
                return Enumerable.Empty<PackageInProject>();
            }

            var itemGroups = project.Elements("ItemGroup");
            var packageRefs = itemGroups.SelectMany(ig => ig.Elements("PackageReference"));

            return packageRefs
                .Select(el => XmlToPackage(el, path))
                .Where(el => el != null)
                .ToList();
        }

        private PackageInProject XmlToPackage(XElement el, PackagePath path)
        {
            try
            {
                var id = el.Attribute("Include")?.Value;
                var version = el.Attribute("Version")?.Value;

                return new PackageInProject(id, version, path);
            }
            catch (Exception ex)
            {
                _logger.Error($"Could not read package from {el} in file {path.FullPath}", ex);
                return null;
            }
        }
    }
}
