using System.Collections.Generic;
using NuKeeper.Nuget;

namespace NuKeeper.RepositoryInspection
{
    public static class ProjectFileReader
    {
        public static IEnumerable<NugetPackage> Read(string fileName)
        {
            return new List<NugetPackage>
            { new NugetPackage("foo", "bar") };
        }
    }
}
