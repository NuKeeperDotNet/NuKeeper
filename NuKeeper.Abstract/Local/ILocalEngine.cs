using System.Threading.Tasks;
using NuKeeper.Abstract.Configuration;

namespace NuKeeper.Abstract.Local
{
    public interface ILocalEngine
    {
        Task Run(ISettingsContainer settings, bool write);
    }
}
