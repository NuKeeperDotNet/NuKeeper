using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.RepositoryInspection;
using System.Collections.Generic;

namespace NuKeeper.Engine.Packages
{
    public interface IPackageUpdateSelection
    {
        IReadOnlyCollection<PackageUpdateSet> SelectTargets(
            ForkData pushFork,
            IReadOnlyCollection<PackageUpdateSet> potentialUpdates,
            FilterSettings filterSettings);
    }
}
