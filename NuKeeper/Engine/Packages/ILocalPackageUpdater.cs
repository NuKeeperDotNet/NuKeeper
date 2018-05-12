using System.Threading.Tasks;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Engine.Packages
{
    public interface ILocalPackageUpdater
    {
        Task Update(PackageUpdateSet updateSet);
    }
}