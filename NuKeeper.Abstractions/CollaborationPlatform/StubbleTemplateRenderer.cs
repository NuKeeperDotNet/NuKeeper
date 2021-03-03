using Stubble.Core;
using Stubble.Core.Builders;
using Stubble.Core.Settings;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public class StubbleTemplateRenderer : ITemplateRenderer
    {
        private readonly StubbleVisitorRenderer _renderer = new StubbleBuilder()
            .Build();

        private readonly RenderSettings _settings = new RenderSettings
        {
            SkipHtmlEncoding = true
        };

        public string Render(string template, object view)
        {
            return _renderer.Render(
                template,
                view,
                _settings
            );
        }
    }
}
