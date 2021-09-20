using NUnit.Framework;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Validators;
using System.Threading.Tasks;

namespace NuKeeper.Tests.Validators
{
    [TestFixture]
    public class StubbleMustacheTemplateValidatorTests
    {
        ITemplateValidator _sut;

        [SetUp]
        public void Initialize()
        {
            _sut = new StubbleMustacheTemplateValidator();
        }

        [TestCase("{{#Invalid}} template", false)]
        [TestCase("{{#InValid}} template {{InValid}}", false)]
        [TestCase("{{Valid}} template", true)]
        [TestCase("{{#Valid}} template {{/Valid}}", true)]
        public async Task ValidateAsync_InvalidAndValidMustacheTemplates_ReturnsExpectedFailureAndSuccessValidationResults(
            string template,
            bool valid
        )
        {
            var result = await _sut.ValidateAsync(template);

            Assert.That(result.IsSuccess, Is.EqualTo(valid));
        }
    }
}
