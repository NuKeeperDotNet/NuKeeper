using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Local
{
    public interface ILocalEngine
    {
        Task Run(SettingsContainer settings, bool write);
        Task RunDowngrade(SettingsContainer settings, bool write);
    }
}
