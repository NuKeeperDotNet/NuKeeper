using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstract.Configuration;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Update.Selection;

namespace NuKeeper.Abstract.Engine.Packages
{
    public interface IPackageUpdateSelection
    {
        Task<IReadOnlyCollection<PackageUpdateSet>> SelectTargets(
            IForkData pushFork,
            IReadOnlyCollection<PackageUpdateSet> potentialUpdates,
            IFilterSettings settings);
    }
}
