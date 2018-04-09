using NuKeeper.Inspection.RepositoryInspection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.Engine.Packages
{
    public interface IExistingBranchFilter
    {
        Task<IEnumerable<PackageUpdateSet>> CanMakeBranchFor(
            ForkData pushFork,
            IEnumerable<PackageUpdateSet> packageUpdateSets);
    }
}
