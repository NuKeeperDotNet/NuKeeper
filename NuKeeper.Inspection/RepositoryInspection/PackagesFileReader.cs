using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public class PackagesFileReader
    {
        private readonly INuKeeperLogger _logger;

        public PackagesFileReader(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<PackageInProject> ReadFile(string baseDirectory, string relativePath)
        {
            var packagePath = new PackagePath(baseDirectory, relativePath, PackageReferenceType.PackagesConfig);
            using (var fileContents = File.OpenRead(packagePath.FullName))
            {
                return Read(fileContents, packagePath);
            }
        }

        public IEnumerable<PackageInProject> Read(Stream fileContents, PackagePath path)
        {
            var xml = XDocument.Load(fileContents);

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

        private PackageInProject XmlToPackage(XElement el, PackagePath path)
        {
            try
            {
                var id = el.Attribute("id")?.Value;
                var version = el.Attribute("version")?.Value;

                return new PackageInProject(id, version, path);

            }
            catch (Exception ex)
            {
                _logger.Error($"Could not read package from {el} in file {path.FullName}", ex);
                return null;
            }
        }
    }
}
