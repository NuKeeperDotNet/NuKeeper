using System.Threading.Tasks;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Abstract.Engine.Packages
{
    public interface IExistingBranchFilter
    {
        Task<bool> CanMakeBranchFor(
            PackageUpdateSet packageUpdateSet,
            IForkData pushFork);
    }
}
