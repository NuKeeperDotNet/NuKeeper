using NuKeeper.Abstractions.CollaborationPlatform;

namespace NuKeeper.Abstractions.CollaborationModels
{
    public class DefaultPullRequestTitleTemplate : UpdateMessageTemplate
    {
        public DefaultPullRequestTitleTemplate()
            : base(new StubbleTemplateRenderer()) { }

        public static string DefaultTemplate { get; } =
            "Automatic update of {{^multipleChanges}}{{#packages}}{{Name}} to {{Version}}{{/packages}}{{/multipleChanges}}{{#multipleChanges}}{{packageCount}} packages{{/multipleChanges}}";

        public string CustomTemplate { get; set; }

        public override string Value => CustomTemplate ?? DefaultTemplate;
    }
}
