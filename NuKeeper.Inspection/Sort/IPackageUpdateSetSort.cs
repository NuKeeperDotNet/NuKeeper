using System.Collections.Generic;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Sort
{
    public interface IPackageUpdateSetSort
    {
        IEnumerable<PackageUpdateSet> Sort(IReadOnlyCollection<PackageUpdateSet> input);
    }
}
