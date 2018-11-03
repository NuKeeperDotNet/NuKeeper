using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Engine
{
    public interface ICollaborationEngine
    {
        Task<int> Run(SettingsContainer settings);
    }
}
