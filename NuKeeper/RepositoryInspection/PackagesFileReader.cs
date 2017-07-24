using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NuKeeper.RepositoryInspection
{
    public static class PackagesFileReader
    {
        public static IEnumerable<PackageInProject> ReadFile(PackagePath path)
        {
            var fileContents = File.ReadAllText(path.FullPath);
            return Read(fileContents, path);
        }

        public static IEnumerable<PackageInProject> Read(string fileContents, PackagePath path)
        {
            var xml = XDocument.Parse(fileContents);

            var packagesNode = xml.Element("packages");
            if (packagesNode == null)
            {
                return Enumerable.Empty<PackageInProject>();
            }

            var packageNodeList = packagesNode.Elements()
                .Where(x => x.Name == "package");

            return packageNodeList
                .Select(el => XmlToPackage(el, path))
                .Where(el => el != null)
                .ToList();
        }

        private static PackageInProject XmlToPackage(XElement el, PackagePath path)
        {
            try
            {
                var id = el.Attribute("id")?.Value;
                var version = el.Attribute("version")?.Value;

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
