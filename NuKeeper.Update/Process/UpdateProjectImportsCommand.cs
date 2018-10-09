using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using NuGet.Configuration;
using NuGet.Versioning;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Update.Process
{
    public class UpdateProjectImportsCommand : IUpdateProjectImportsCommand
    {
        public async Task Invoke(PackageInProject currentPackage,
            NuGetVersion newVersion, PackageSource packageSource, NuGetSources allSources)
        {
            var projectsToUpdate = new Stack<string>();
            projectsToUpdate.Push(currentPackage.Path.FullName);

            while (projectsToUpdate.TryPop(out var currentProject))
            {
                using (var projectContents = File.Open(currentProject, FileMode.Open, FileAccess.ReadWrite))
                {
                    var projectsToCheck = await UpdateConditionsOnProjects(projectContents)
                        .ConfigureAwait(false);

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
            }
        }

        private static async Task<IEnumerable<string>> UpdateConditionsOnProjects(Stream fileContents)
        {
            var xml = XDocument.Load(fileContents);
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
                fileContents.Seek(0, SeekOrigin.Begin);
                await xml.SaveAsync(fileContents, SaveOptions.None, CancellationToken.None)
                    .ConfigureAwait(false);
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
