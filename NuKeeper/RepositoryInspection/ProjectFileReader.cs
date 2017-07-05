using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NuKeeper.Nuget;

namespace NuKeeper.RepositoryInspection
{
    public static class ProjectFileReader
    {
        public static IEnumerable<NugetPackage> ReadFile(string fileName)
        {
            var fileContents = File.ReadAllText(fileName);
            return Read(fileContents);
        }

        public static IEnumerable<NugetPackage> Read(string fileContents)
        {
            var xml = XDocument.Parse(fileContents);
            var project = xml.Element("Project");

            var itemGroups = project.Elements("ItemGroup");
            var packageRefs = itemGroups.SelectMany(ig => ig.Elements("PackageReference"));

            return packageRefs
                .Select(XmlToPackage)
                .ToList();
        }

        private static NugetPackage XmlToPackage(XElement el)
        {
            var id = el.Attribute("Include")?.Value;
            var version = el.Attribute("Version")?.Value;

            return new NugetPackage(id, version);
        }
    }
}
