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
            var packagesNode = xml.Element("Project")?.Element("ItemGroup");
            if (packagesNode == null)
            {
                return;
            }

            var packageNodeList = packagesNode.Elements()
                .Where(x => x.Name == "PackageReference" && (x.Attributes("Include")
                                                                 .Any(a => a.Value == currentPackage.Id)
                                                             || x.Attributes("Update")
                                                                 .Any(a => a.Value == currentPackage.Id)));

            foreach (var dependencyToUpdate in packageNodeList)
            {
                _logger.Detailed(
                    $"Updating directory-level dependencies: {currentPackage.Id} in path {currentPackage.Path.FullName}");
                dependencyToUpdate.Attribute("Version").Value = newVersion.ToString();
            }

            xml.Save(fileContents);
        }
    }
}
