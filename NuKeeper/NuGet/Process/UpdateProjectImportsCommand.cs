using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using NuGet.Versioning;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.NuGet.Process
{
    public class UpdateProjectImportsCommand : IPackageCommand
    {
        public async Task Invoke(NuGetVersion newVersion, string packageSource, PackageInProject currentPackage)
        {
            using (var projectContents = File.Open(currentPackage.Path.FullName, FileMode.Open, FileAccess.ReadWrite))
            {
                await UpdateConditionsOnProjects(projectContents);
            }
        }

        private async Task UpdateConditionsOnProjects(Stream fileContents)
        {
            var xml = XDocument.Load(fileContents);
            var ns = xml.Root.GetDefaultNamespace();

            var project = xml.Element(ns + "Project");

            if (project == null)
            {
                return;
            }

            var imports = project.Elements(ns + "Import");
            var importsWithToolsPath = imports.Where(i => i.Attributes("Project").Any(a => a.Value.Contains("$(VSToolsPath)"))).ToList();
            var importsWithoutCondition = importsWithToolsPath.Where(i => !i.Attributes("Condition").Any());
            var importsWithBrokenVsToolsCondition = importsWithToolsPath.Where(i =>
                i.Attributes("Condition").Any(a => a.Value == "\'$(VSToolsPath)\' != \'\'"));

            bool saveRequired = false;
            foreach (var importToFix in importsWithBrokenVsToolsCondition.Concat(importsWithoutCondition))
            {
                saveRequired = true;
                UpdateImportNode(importToFix);
            }

            if(saveRequired)
            {
                fileContents.Seek(0, SeekOrigin.Begin);
                await xml.SaveAsync(fileContents, SaveOptions.None, CancellationToken.None);
            }
        }

        private static void UpdateImportNode(XElement importToFix)
        {
            var importPath = importToFix.Attribute("Project").Value;
            var condition = $"Exists('{importPath}')";
            if (!importToFix.Attributes("Condition").Any())
            {
                importToFix.Add(new XAttribute("Condition", condition));
            }
            else
            {
                importToFix.Attribute("Condition").Value = condition;
            }
        }
    }
}
