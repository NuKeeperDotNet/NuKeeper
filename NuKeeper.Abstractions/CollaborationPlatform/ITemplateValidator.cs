using NuKeeper.Abstractions.Configuration;
using System.Threading.Tasks;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface ITemplateValidator
    {
#pragma warning disable CA1716 // Identifiers should not match keywords
        Task<ValidationResult> ValidateAsync(string template);
#pragma warning restore CA1716 // Identifiers should not match keywords
    }
}
