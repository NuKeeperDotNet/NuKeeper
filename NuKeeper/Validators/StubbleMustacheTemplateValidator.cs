using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using Stubble.Core.Exceptions;
using Stubble.Core.Parser;
using System.Threading.Tasks;

namespace NuKeeper.Validators
{
    public class StubbleMustacheTemplateValidator : ITemplateValidator
    {
        public Task<ValidationResult> ValidateAsync(string template)
        {
            try
            {
                MustacheParser.Parse(template);
            }
            catch (StubbleException ex)
            {
                return Task.FromResult(ValidationResult.Failure(ex.Message));
            }

            return Task.FromResult(ValidationResult.Success);
        }
    }
}
