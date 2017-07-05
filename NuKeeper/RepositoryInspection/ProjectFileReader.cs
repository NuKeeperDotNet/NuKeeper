using System.Collections.Generic;
using System.IO;
using NuKeeper.Nuget;

namespace NuKeeper.RepositoryInspection
{
    public static class ProjectFileReader
    {
        public static IEnumerable<NugetPackage> ReadFile(string fileName)
        {
            var fileContents = File.ReadAllText(fileName);
            return Read(fileContents);
        }

        public static IEnumerable<NugetPackage> Read(string fileContents)
        {
            return new List<NugetPackage>
                {new NugetPackage("foo", "bar", "fish")};
        }
    }
}
