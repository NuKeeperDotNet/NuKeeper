using NuKeeper.Inspection.RepositoryInspection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.Update.Selection
{
    public interface IUpdateSelection
    {
        Task<IReadOnlyCollection<PackageUpdateSet>> Filter(
            IReadOnlyCollection<PackageUpdateSet> potentialUpdates,
            IFilterSettings settings,
            Func<PackageUpdateSet, Task<bool>> remoteCheck);
    }
}
