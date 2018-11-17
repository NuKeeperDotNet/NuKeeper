using NuKeeper.Inspection.RepositoryInspection;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;

namespace NuKeeper.Engine.Packages
{
    public interface IExistingBranchFilter
    {
        Task<bool> CanMakeBranchFor(
            PackageUpdateSet packageUpdateSet,
            ForkData pushFork);
    }
}
