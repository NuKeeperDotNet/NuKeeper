using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Update.Selection
{
    public interface IUpdateSelection
    {
        IReadOnlyCollection<PackageUpdateSet> Filter(
            IReadOnlyCollection<PackageUpdateSet> potentialUpdates,
            FilterSettings settings);
    }
}
