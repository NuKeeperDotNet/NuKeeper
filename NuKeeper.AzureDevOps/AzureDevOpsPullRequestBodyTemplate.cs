using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsPullRequestBodyTemplate : UpdateMessageTemplate
    {
        // Azure DevOps allows a maximum of 4000 characters to be used in a pull request description:
        // https://visualstudio.uservoice.com/forums/330519-azure-devops-formerly-visual-studio-team-services/suggestions/20217283-raise-the-character-limit-for-pull-request-descrip
        private const int MaxCharacterCount = 4000;

        public AzureDevOpsPullRequestBodyTemplate()
            : base(new StubbleTemplateRenderer()) { }

        public static string DefaultTemplate { get; } =
@"{{#multipleChanges}}{{packageCount}} packages were updated in {{projectsUpdated}} project{{#multipleProjects}}s{{/multipleProjects}}:
{{#packages}}| {{Name}} {{/packages}}|

## Details of updated packages

{{/multipleChanges}}
{{#packages}}NuKeeper has generated a {{ActualChange}} update of `{{Name}}` to `{{Version}}`{{^MultipleUpdates}} from `{{FromVersion}}`{{/MultipleUpdates}}
{{#MultipleUpdates}}{{ProjectsUpdated}} versions of `{{Name}}` were found in use: {{#Updates}}`{{FromVersion}}`{{^Last}}, {{/Last}}{{/Updates}}{{/MultipleUpdates}}
{{#Publication}}`{{Name}} {{Version}}` was published at `{{Date}}`, {{Ago}}{{/Publication}}
{{#LatestVersion}}There is also a higher version, `{{Name}} {{Version}}`{{#Publication}} published at `{{Date}}`, {{Ago}}{{/Publication}}, but this was not applied as only `{{AllowedChange}}` version changes are allowed.
{{/LatestVersion}}
### {{ProjectsUpdated}} project update{{#MultipleProjectsUpdated}}s{{/MultipleProjectsUpdated}}:
| Project   | Package   | From   | To   |
|:----------|:----------|-------:|-----:|
{{#Updates}}
| `{{SourceFilePath}}` | `{{Name}}` | {{#IsFromNuget}}[{{FromVersion}}]({{FromUrl}}) | [{{ToVersion}}]({{Url}}) |{{/IsFromNuget}}{{^IsFromNuget}}{{FromVersion}} | {{ToVersion}} |{{/IsFromNuget}}
{{/Updates}}
{{#IsFromNuget}}

[{{Name}} {{Version}} on NuGet.org]({{Url}})
{{/IsFromNuget}}
{{/packages}}

{{#footer}}
{{WarningMessage}}
**NuKeeper**: {{NuKeeperUrl}}
{{/footer}}
";

        public string CustomTemplate { get; set; }

        public override string Value => CustomTemplate ?? DefaultTemplate;

        public override string Output()
        {
            var output = base.Output();

            if (output.Length > MaxCharacterCount)
            {
                //todo: improve
                return output.Substring(0, MaxCharacterCount - 3) + "...";
            }

            return output;
        }
    }
}
