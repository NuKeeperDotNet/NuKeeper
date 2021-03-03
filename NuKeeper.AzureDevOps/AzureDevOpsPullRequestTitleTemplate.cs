using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsPullRequestTitleTemplate : UpdateMessageTemplate
    {
        private const string CommitEmoji = "📦";

        public AzureDevOpsPullRequestTitleTemplate()
            : base(new StubbleTemplateRenderer())
        {
            PackageEmoji = CommitEmoji;
        }

        public static string DefaultTemplate { get; } =
            "{{packageEmoji}} Automatic update of {{^multipleChanges}}{{#packages}}{{Name}} to {{Version}}{{/packages}}{{/multipleChanges}}{{#multipleChanges}}{{packageCount}} packages{{/multipleChanges}}";

        public string CustomTemplate { get; set; }

        public override string Value => CustomTemplate ?? DefaultTemplate;

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
