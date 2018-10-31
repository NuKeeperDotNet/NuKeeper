using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Engine
{
    public interface IGitHubEngine
    {
        Task<int> Run(SettingsContainer settings);
    }
}
