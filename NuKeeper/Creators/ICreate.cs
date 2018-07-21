using NuKeeper.Configuration;

namespace NuKeeper.Creators
{
    public interface ICreate<out T>
    {
        T Create(SettingsContainer settings);
    }
}
