using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Abstractions
{
    public interface IEngine
    {
        Task<int> Run(ISettingsContainer settings);
    }
}
