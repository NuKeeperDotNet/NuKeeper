using System.Threading.Tasks;
using NuKeeper.Configuration;

namespace NuKeeper.Engine
{
    public interface IGitHubEngine
    {
        Task<int> Run(SettingsContainer settings);
    }
}
