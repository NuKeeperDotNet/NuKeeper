using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NuKeeper.RepositoryInspection
{
    public static class PackagesFileReader
    {
        public static IEnumerable<NuGetPackage> ReadFile(PackagePath path)
        {
            var fileContents = File.ReadAllText(path.FullPath);
            return Read(fileContents, path);
        }

        public static IEnumerable<NuGetPackage> Read(string fileContents, PackagePath path)
        {
            var xml = XDocument.Parse(fileContents);

            var packagesNode = xml.Element("packages");
            if (packagesNode == null)
            {
                return Enumerable.Empty<NuGetPackage>();
            }

            var packageNodeList = packagesNode.Elements()
                .Where(x => x.Name == "package");

            return packageNodeList
                .Select(el => XmlToPackage(el, path))
                .ToList();
        }

        private static NuGetPackage XmlToPackage(XElement el, PackagePath path)
        {
            var id = el.Attribute("id")?.Value;
            var version = el.Attribute("version")?.Value;

            return new NuGetPackage(id, version, path, PackageType.PackagesConfig);
        }
    }
}
