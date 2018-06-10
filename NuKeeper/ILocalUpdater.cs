using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper
{
    public interface ILocalUpdater
    {
        Task ApplyAnUpdate(IReadOnlyCollection<PackageUpdateSet> updates);
    }
}
