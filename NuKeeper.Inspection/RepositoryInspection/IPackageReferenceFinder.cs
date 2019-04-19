using System.Collections.Generic;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public interface IPackageReferenceFinder
    {
        IReadOnlyCollection<PackageInProject> ReadFile(string baseDirectory, string relativePath);

        IReadOnlyCollection<string> GetFilePatterns();
    }
}
