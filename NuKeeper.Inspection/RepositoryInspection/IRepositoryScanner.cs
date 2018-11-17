using System.Collections.Generic;
using NuKeeper.Abstractions.Inspections.Files;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public interface IRepositoryScanner
    {
        IReadOnlyCollection<PackageInProject> FindAllNuGetPackages(IFolder workingFolder);
    }
}
