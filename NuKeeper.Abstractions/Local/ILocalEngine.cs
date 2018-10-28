using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Abstractions.Local
{
    public interface ILocalEngine
    {
        Task Run(ISettingsContainer settings, bool write);
    }
}
