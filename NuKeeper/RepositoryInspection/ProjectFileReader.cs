using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NuKeeper.RepositoryInspection
{
    public static class ProjectFileReader
    {
        public static IEnumerable<NugetPackage> ReadFile(PackagePath path)
        {
            var fileContents = File.ReadAllText(path.FullPath);
            return Read(fileContents, path);
        }

        public static IEnumerable<NugetPackage> Read(string fileContents, PackagePath path)
        {
            var xml = XDocument.Parse(fileContents);
            var project = xml.Element("Project");

            if (project == null)
            {
                return Enumerable.Empty<NugetPackage>();
            }

            var itemGroups = project.Elements("ItemGroup");
            var packageRefs = itemGroups.SelectMany(ig => ig.Elements("PackageReference"));

            return packageRefs
                .Select(el => XmlToPackage(el, path))
                .ToList();
        }

        private static NugetPackage XmlToPackage(XElement el, PackagePath path)
        {
            var id = el.Attribute("Include")?.Value;
            var version = el.Attribute("Version")?.Value;

            return new NugetPackage(id, version, path);
        }
    }
}
