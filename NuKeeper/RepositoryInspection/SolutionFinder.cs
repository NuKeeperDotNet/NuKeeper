using System;
using System.Collections.Generic;
using System.IO;

namespace NuKeeper.RepositoryInspection
{
    public static class SolutionFinder
    {
        public static IEnumerable<string> FindSolutions(string rootDirectory)
        {
            if (!Directory.Exists(rootDirectory))
            {
              throw new Exception($"No such directory: '{rootDirectory}'");
            }

            return Directory.GetFiles(rootDirectory, "*.sln", SearchOption.AllDirectories);
        }
    }
}