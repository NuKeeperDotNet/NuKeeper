using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NuKeeper.RepositoryInspection
{
    public static class ProjectFileReader
    {
        public static IEnumerable<NuGetPackage> ReadFile(string fileName)
        {
            var fileContents = File.ReadAllText(fileName);
            return Read(fileContents);
        }

        public static IEnumerable<NuGetPackage> Read(string fileContents)
        {
            var xml = XDocument.Parse(fileContents);
            var project = xml.Element("Project");

            if (project == null)
            {
                return Enumerable.Empty<NuGetPackage>();
            }

            var itemGroups = project.Elements("ItemGroup");
            var packageRefs = itemGroups.SelectMany(ig => ig.Elements("PackageReference"));

            return packageRefs
                .Select(XmlToPackage)
                .ToList();
        }

        private static NuGetPackage XmlToPackage(XElement el)
        {
            var id = el.Attribute("Include")?.Value;
            var version = el.Attribute("Version")?.Value;

            return new NuGetPackage(id, version);
        }
    }
}
