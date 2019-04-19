using System.Collections.Generic;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Inspection.Sort
{
    public interface IPackageUpdateSetSort
    {
        IEnumerable<PackageUpdateSet> Sort(IReadOnlyCollection<PackageUpdateSet> input);
    }
}
