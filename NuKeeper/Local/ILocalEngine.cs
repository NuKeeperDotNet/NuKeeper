using System.Threading.Tasks;
using NuKeeper.Configuration;

namespace NuKeeper.Local
{
    public interface ILocalEngine
    {
        Task Run(SettingsContainer settings, bool write);
    }
}
