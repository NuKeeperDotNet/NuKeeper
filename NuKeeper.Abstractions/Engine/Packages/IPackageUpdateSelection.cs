using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Update.Selection;

namespace NuKeeper.Abstractions.Engine.Packages
{
    public interface IPackageUpdateSelection
    {
        Task<IReadOnlyCollection<PackageUpdateSet>> SelectTargets(
            ForkData pushFork,
            IReadOnlyCollection<PackageUpdateSet> potentialUpdates,
            FilterSettings settings);
    }
}
