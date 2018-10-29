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
            FilterSettings settings,
            Func<PackageUpdateSet, Task<bool>> remoteCheck);
    }
}
