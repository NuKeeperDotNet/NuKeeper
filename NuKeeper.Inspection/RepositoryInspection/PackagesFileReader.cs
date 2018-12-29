using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public class PackagesFileReader : IPackageReferenceFinder
    {
        private readonly INuKeeperLogger _logger;

        public PackagesFileReader(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public IReadOnlyCollection<PackageInProject> ReadFile(string baseDirectory, string relativePath)
        {
            var packagePath = new PackagePath(baseDirectory, relativePath, PackageReferenceType.PackagesConfig);
            try
            {
                using (var fileContents = File.OpenRead(packagePath.FullName))
                {
                    return Read(fileContents, packagePath);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Unable to parse file {packagePath.FullName}", ex);
            }
        }

        public IReadOnlyCollection<string> GetFilePatterns()
        {
            return new[] {"packages.config"};
        }

        public IReadOnlyCollection<PackageInProject> Read(Stream fileContents, PackagePath path)
        {
            var xml = XDocument.Load(fileContents);

            var packagesNode = xml.Element("packages");
            if (packagesNode == null)
            {
                return Array.Empty<PackageInProject>();
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

                if (string.IsNullOrWhiteSpace(version))
                {
                    _logger.Normal($"Skipping package '{id}' with no version specified.");
                    return null;
                }

                var packageVersionRange = PackageVersionRange.Read(id, version);

                if (packageVersionRange == null)
                {
                    _logger.Normal($"Skipping package '{id}' with version '{version}' that could not be parsed.");
                    return null;
                }

                var singleVersion = packageVersionRange.SingleVersionIdentity();

                if (singleVersion == null)
                {
                    _logger.Normal($"Skipping package '{id}' with version range '{version}' that is not a single version.");
                    return null;
                }

                return new PackageInProject(singleVersion, path, null);

            }
            catch (Exception ex)
            {
                _logger.Error($"Could not read package from {el} in file {path.FullName}", ex);
                return null;
            }
        }
    }
}
