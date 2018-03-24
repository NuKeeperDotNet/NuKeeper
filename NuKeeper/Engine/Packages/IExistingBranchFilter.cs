using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine.Packages
{
    public interface IExistingBranchFilter
    {
        Task<IEnumerable<PackageUpdateSet>> CanMakeBranchFor(
            ForkData pushFork,
            IEnumerable<PackageUpdateSet> packageUpdateSets);
    }
}
