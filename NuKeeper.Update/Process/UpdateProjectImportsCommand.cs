using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using NuGet.Configuration;
using NuGet.Versioning;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Update.Process
{
    public class UpdateProjectImportsCommand : IUpdateProjectImportsCommand
    {
        public Task Invoke(PackageInProject currentPackage,
            NuGetVersion newVersion, PackageSource packageSource, NuGetSources allSources)
        {
            var projectsToUpdate = new Stack<string>();
            projectsToUpdate.Push(currentPackage.Path.FullName);

            while (projectsToUpdate.Count > 0)
            {
                var currentProject = projectsToUpdate.Pop();

                XDocument xml;
                using (var projectContents = File.Open(currentProject, FileMode.Open, FileAccess.ReadWrite))
                {
                    xml = XDocument.Load(projectContents);
                }

                var projectsToCheck = UpdateConditionsOnProjects(xml, currentProject);

                foreach (var potentialProject in projectsToCheck)
                {
                    var fullPath =
                        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(currentProject), potentialProject));
                    if (File.Exists(fullPath))
                    {
                        projectsToUpdate.Push(fullPath);
                    }
                }
            }

            return Task.CompletedTask;
        }

        private static IEnumerable<string> UpdateConditionsOnProjects(XDocument xml, string savePath)
        {
            var ns = xml.Root.GetDefaultNamespace();

            var project = xml.Element(ns + "Project");

            if (project == null)
            {
                return Enumerable.Empty<string>();
            }

            var imports = project.Elements(ns + "Import");
            var importsWithToolsPath = imports
                .Where(i => i.Attributes("Project").Any(a => a.Value.Contains("$(VSToolsPath)")))
                .ToList();

            var importsWithoutCondition = importsWithToolsPath.Where(i => !i.Attributes("Condition").Any());
            var importsWithBrokenVsToolsCondition = importsWithToolsPath.Where(i =>
                i.Attributes("Condition").Any(a => a.Value == "\'$(VSToolsPath)\' != \'\'"));

            var saveRequired = false;
            foreach (var importToFix in importsWithBrokenVsToolsCondition.Concat(importsWithoutCondition))
            {
                saveRequired = true;
                UpdateImportNode(importToFix);
            }

            if (saveRequired)
            {
                using (var xmlOutput = File.Open(savePath, FileMode.Truncate))
                {
                    xml.Save(xmlOutput);
                }
            }

            return FindProjectReferences(project, ns);
        }

        private static IEnumerable<string> FindProjectReferences(XElement project, XNamespace ns)
        {
            var itemGroups = project.Elements(ns + "ItemGroup");
            var projectReferences = itemGroups.SelectMany(ig => ig.Elements(ns + "ProjectReference"));
            var includes = projectReferences.Attributes("Include").Select(a => a.Value);
            return includes;
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
