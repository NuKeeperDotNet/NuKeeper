using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using NuGet.Configuration;
using NuGet.Versioning;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Update.Process
{
    public class UpdateNuspecCommand : IUpdateNuspecCommand
    {
        private readonly INuKeeperLogger _logger;

        public UpdateNuspecCommand(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public Task Invoke(PackageInProject currentPackage,
            NuGetVersion newVersion, PackageSource packageSource, NuGetSources allSources)
        {
            using (var nuspecContents = File.Open(currentPackage.Path.FullName, FileMode.Open, FileAccess.ReadWrite))
            {
                UpdateNuspec(nuspecContents, newVersion, currentPackage);
                return Task.CompletedTask;
            }
        }

        private void UpdateNuspec(FileStream fileContents, NuGetVersion newVersion,
            PackageInProject currentPackage)
        {
            var xml = XDocument.Load(fileContents);

            var packagesNode = xml.Element("package")?.Element("metadata")?.Element("dependencies");
            if (packagesNode == null)
            {
                return;
            }

            var packageNodeList = packagesNode.Elements()
                .Where(x => x.Name == "dependency" && x.Attributes("id")
                .Any(a => a.Value == currentPackage.Id));

            foreach (var dependencyToUpdate in packageNodeList)
            {
                _logger.Detailed($"Updating nuspec depenencies: {currentPackage.Id} in path {currentPackage.Path.FullName}");
                dependencyToUpdate.Attribute("version").Value = newVersion.ToString();
            }

            fileContents.Seek(0, SeekOrigin.Begin);
            
            xml.Save(fileContents);
        }
    }
}
