using System.Threading.Tasks;
using NuKeeper.Abstract.Configuration;

namespace NuKeeper.Abstract
{
    public interface IEngine
    {
        Task<int> Run(ISettingsContainer settings);
    }
}
