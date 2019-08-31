using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NuKeeper.Inspection.RepositoryInspection
{
    internal static class MsBuildExtensions
    {
        public static IEnumerable<string> ExtractImports(XElement project, string relativePath)
        {
            var msBuildBaseDirectory =
                (relativePath.EndsWith(Path.AltDirectorySeparatorChar.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase) || relativePath.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
                ? relativePath
                : $"{relativePath}{Path.DirectorySeparatorChar}";

            var centralPackagesFile = project.Elements("PropertyGroup")
                .SelectMany(pg => pg.Elements("CentralPackagesFile"))
                .FirstOrDefault();
            if (centralPackagesFile != null)
            {
                yield return centralPackagesFile.Value
                    .Replace("$(MSBuildThisFileDirectory)", msBuildBaseDirectory)
                    .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            var importNodes = project.Elements("Import");
            foreach (var import in importNodes)
            {

                var projectAttribute = import.Attribute("Project");
                if (projectAttribute == null) continue;
                var importPath = projectAttribute.Value
                    .Replace("$(MSBuildThisFileDirectory)", msBuildBaseDirectory)
                    .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                yield return importPath;
            }
        }
    }
}
