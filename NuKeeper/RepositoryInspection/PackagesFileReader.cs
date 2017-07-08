using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NuKeeper.RepositoryInspection
{
    public static class PackagesFileReader
    {
        public static IEnumerable<NuGetPackage> ReadFile(string fileName)
        {
            var fileContents = File.ReadAllText(fileName);
            return Read(fileContents);
        }

        public static IEnumerable<NuGetPackage> Read(string fileContents)
        {
            var xml = XDocument.Parse(fileContents);

            var packagesNode = xml.Element("packages");
            if (packagesNode == null)
            {
                return Enumerable.Empty<NuGetPackage>();
            }

            var packageNodeList = packagesNode.Elements()
                .Where(x => x.Name == "package");

            return packageNodeList.Select(XmlToPackage).ToList();
        }

        private static NuGetPackage XmlToPackage(XElement el)
        {
            var id = el.Attribute("id")?.Value;
            var version = el.Attribute("version")?.Value;

            return new NuGetPackage(id, version);
        }
    }
}
