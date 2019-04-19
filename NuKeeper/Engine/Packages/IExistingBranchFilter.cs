using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Engine.Packages
{
    public interface IExistingBranchFilter
    {
        Task<bool> CanMakeBranchFor(
            PackageUpdateSet packageUpdateSet,
            ForkData pushFork);
    }
}
