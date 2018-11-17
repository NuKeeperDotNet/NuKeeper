using NuKeeper.Inspection.RepositoryInspection;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Engine.Packages
{
    public interface IPackageUpdateSelection
    {
        Task<IReadOnlyCollection<PackageUpdateSet>> SelectTargets(
            ForkData pushFork,
            IReadOnlyCollection<PackageUpdateSet> potentialUpdates,
            FilterSettings settings);
    }
}
