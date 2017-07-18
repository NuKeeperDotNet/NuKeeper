using System.Collections.Generic;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public interface IPackageUpdateSelection
    {
        List<PackageUpdateSet> SelectTargets(IEnumerable<PackageUpdateSet> potentialUpdates);
    }
}