using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using NuGet.Configuration;
using NuGet.Versioning;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Update.Process
{
    public class UpdateDirectoryBuildTargetsCommand : IUpdateDirectoryBuildTargetsCommand
    {
        private readonly INuKeeperLogger _logger;

        public UpdateDirectoryBuildTargetsCommand(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public Task Invoke(PackageInProject currentPackage, NuGetVersion newVersion, PackageSource packageSource,
            NuGetSources allSources)
        {
            XDocument xml;
            using (var xmlInput = File.OpenRead(currentPackage.Path.FullName))
            {
                xml = XDocument.Load(xmlInput);
            }

            using (var xmlOutput = File.Open(currentPackage.Path.FullName, FileMode.Truncate))
            {
                UpdateFile(xmlOutput, newVersion, currentPackage, xml);
            }

            return Task.CompletedTask;
        }

        private void UpdateFile(Stream fileContents, NuGetVersion newVersion,
            PackageInProject currentPackage, XDocument xml)
        {
            var packagesNode = xml.Element("Project")?.Elements("ItemGroup");
            if (packagesNode == null)
            {
                return;
            }

            var packageReferenceList = packagesNode.Elements("PackageReference")
                .Where(x =>
                    (x.Attributes("Include").Any(a => a.Value.Equals(currentPackage.Id, StringComparison.InvariantCultureIgnoreCase))
                  || x.Attributes("Update").Any(a => a.Value.Equals(currentPackage.Id, StringComparison.InvariantCultureIgnoreCase))));

            foreach (var dependencyToUpdate in packageReferenceList)
            {
                _logger.Detailed(
                    $"Updating directory-level dependencies: {currentPackage.Id} in path {currentPackage.Path.FullName}");
                dependencyToUpdate.Attribute("Version").Value = newVersion.ToString();
            }

            var packageDownloadList = packagesNode.Elements("PackageDownload")
                .Where(x =>
                    (x.Attributes("Include").Any(a => a.Value.Equals(currentPackage.Id, StringComparison.InvariantCultureIgnoreCase))
                  || x.Attributes("Update").Any(a => a.Value.Equals(currentPackage.Id, StringComparison.InvariantCultureIgnoreCase))));

            foreach (var dependencyToUpdate in packageDownloadList)
            {
                _logger.Detailed(
                    $"Updating directory-level dependencies: {currentPackage.Id} in path {currentPackage.Path.FullName}");
                dependencyToUpdate.Attribute("Version").Value = $"[{newVersion}]";
            }

            xml.Save(fileContents);
        }
    }
}
