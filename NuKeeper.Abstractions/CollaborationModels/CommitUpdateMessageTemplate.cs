using NuKeeper.Abstractions.CollaborationPlatform;

namespace NuKeeper.Abstractions.CollaborationModels
{
    public class CommitUpdateMessageTemplate : UpdateMessageTemplate
    {
        private const string CommitEmoji = "📦";

        public CommitUpdateMessageTemplate()
            : base(new StubbleTemplateRenderer())
        {
            PackageEmoji = CommitEmoji;
        }

        public static string DefaultTemplate { get; } =
            "{{#packageEmoji}}{{packageEmoji}} {{/packageEmoji}}Automatic update of {{^multipleChanges}}{{#packages}}{{Name}} to {{Version}}{{/packages}}{{/multipleChanges}}{{#multipleChanges}}{{packageCount}} packages{{/multipleChanges}}";

        public override string Value
        {
            get
            {
                return CustomTemplate ?? DefaultTemplate;
            }
        }

        public string CustomTemplate { get; set; }

        public object PackageEmoji
        {
            get
            {
                Context.TryGetValue(Constants.Template.PackageEmoji, out var packageEmoji);
                return packageEmoji;
            }
            set
            {
                Context[Constants.Template.PackageEmoji] = value;
            }
        }

        public override void Clear()
        {
            base.Clear();
            PackageEmoji = CommitEmoji;
        }
    }
}
