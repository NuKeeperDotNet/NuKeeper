using System.Collections.Generic;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public interface IPackageReferenceFinder
    {
        IReadOnlyCollection<PackageInProject> ReadFile(string baseDirectory, string relativePath);

        IReadOnlyCollection<string> GetFilePatterns();
    }
}
