using System.Collections.Generic;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Engine.Sort
{
    public interface IPackageUpdateSetSort
    {
        IEnumerable<PackageUpdateSet> Sort(IReadOnlyCollection<PackageUpdateSet> input);
    }
}
