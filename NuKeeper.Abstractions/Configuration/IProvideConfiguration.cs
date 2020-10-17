using System.Threading.Tasks;

namespace NuKeeper.Abstractions.Configuration
{
    public interface IProvideConfiguration
    {
        Task<ValidationResult> ProvideAsync(SettingsContainer settings);
    }
}
