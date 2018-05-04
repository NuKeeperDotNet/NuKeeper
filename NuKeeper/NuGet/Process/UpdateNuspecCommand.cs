using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using NuGet.Versioning;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.NuGet.Process
{
    public class UpdateNuspecCommand : IPackageCommand
    {
        private readonly INuKeeperLogger _logger;

        public UpdateNuspecCommand(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public async Task Invoke(NuGetVersion newVersion, string packageSource, PackageInProject currentPackage)
        {
            using (var nuspecContents = File.Open(currentPackage.Path.FullName, FileMode.Open, FileAccess.ReadWrite))
            {
                await UpdateNuspec(nuspecContents, newVersion, currentPackage);
            }
        }

        private async Task UpdateNuspec(FileStream fileContents, NuGetVersion newVersion,
            PackageInProject currentPackage)
        {
            var xml = XDocument.Load(fileContents);

            var packagesNode = xml.Element("package")?.Element("metadata")?.Element("dependencies");
            if (packagesNode == null)
            {
                return;
            }

            var packageNodeList = packagesNode.Elements()
                .Where(x => x.Name == "dependency" && x.Attributes("id").Any(a => a.Value == currentPackage.Id));

            foreach (var dependencyToUpdate in packageNodeList)
            {
                _logger.Verbose($"Updating nuspec depenencies: {currentPackage.Id} in path {currentPackage.Path.FullName}");
                dependencyToUpdate.Attribute("version").Value = newVersion.ToString();
            }

            fileContents.Seek(0, SeekOrigin.Begin);
            await xml.SaveAsync(fileContents, SaveOptions.None, CancellationToken.None);
        }
    }
}
