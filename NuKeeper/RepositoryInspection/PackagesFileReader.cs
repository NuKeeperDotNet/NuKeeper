using System;
using System.Collections.Generic;
using NuKeeper.Nuget;

namespace NuKeeper.RepositoryInspection
{
    public static class PackagesFileReader
    {
        public static IEnumerable<NugetPackage> Read(string fileName)
        {
            return new List<NugetPackage>();
        }
    }
}
