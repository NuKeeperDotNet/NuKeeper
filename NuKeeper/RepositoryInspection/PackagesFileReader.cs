using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NuKeeper.Nuget;

namespace NuKeeper.RepositoryInspection
{
    public static class PackagesFileReader
    {
        public static IEnumerable<NugetPackage> ReadFile(string fileName)
        {
            var fileContents = File.ReadAllText(fileName);
            return Read(fileContents);
        }

        public static IEnumerable<NugetPackage> Read(string fileContents)
        {
            var xml = XDocument.Parse(fileContents);

            var packagesNode = xml.Element("packages");
            if (packagesNode == null)
            {
                return new List<NugetPackage>();
            }

            var packageNodeList = packagesNode.Elements()
                .Where(x => x.Name == "package");

            return packageNodeList.Select(XmlToPackage);
        }

        private static NugetPackage XmlToPackage(XElement el)
        {
            var id = el.Attribute("id")?.Value;
            var version = el.Attribute("version")?.Value;
            var targetFramework = el.Attribute("targetFramework")?.Value;

            return new NugetPackage(id, version, targetFramework);
        }
    }
}
