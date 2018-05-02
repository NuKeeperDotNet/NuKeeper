using System.Collections.Generic;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public interface IPackageReferenceFinder
    {
        IEnumerable<PackageInProject> ReadFile(string baseDirectory, string relativePath);

        IEnumerable<string> GetFilePatterns();
    }
}
