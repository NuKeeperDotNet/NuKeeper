using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NuKeeper.RepositoryInspection
{
    public static class ProjectFileReader
    {
        public static IEnumerable<PackageInProject> ReadFile(PackagePath path)
        {
            var fileContents = File.ReadAllText(path.FullPath);
            return Read(fileContents, path);
        }

        public static IEnumerable<PackageInProject> Read(string fileContents, PackagePath path)
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

        private static PackageInProject XmlToPackage(XElement el, PackagePath path)
        {
            try
            {
                var id = el.Attribute("Include")?.Value;
                var version = el.Attribute("Version")?.Value;

                return new PackageInProject(id, version, path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not read package from {el}: {ex.Message}");
                return null;
            }
        }
    }
}
