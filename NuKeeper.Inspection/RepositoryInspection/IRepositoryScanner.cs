using System.Collections.Generic;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public interface IRepositoryScanner
    {
        IReadOnlyCollection<PackageInProject> FindAllNuGetPackages(IFolder workingFolder);
    }
}
