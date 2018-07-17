using NuKeeper.Configuration;

namespace NuKeeper
{
    public interface ICreate<T>
    {
        T Create(SettingsContainer settings);
    }
}
