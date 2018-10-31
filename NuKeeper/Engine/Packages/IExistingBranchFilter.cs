using NuKeeper.Inspection.RepositoryInspection;
using System.Threading.Tasks;
using NuKeeper.Abstractions.DTOs;

namespace NuKeeper.Engine.Packages
{
    public interface IExistingBranchFilter
    {
        Task<bool> CanMakeBranchFor(
            PackageUpdateSet packageUpdateSet,
            ForkData pushFork);
    }
}
