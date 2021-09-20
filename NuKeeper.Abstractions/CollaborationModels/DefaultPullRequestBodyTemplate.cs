using NuKeeper.Abstractions.CollaborationPlatform;

namespace NuKeeper.Abstractions.CollaborationModels
{
    public class DefaultPullRequestBodyTemplate : UpdateMessageTemplate
    {
        public DefaultPullRequestBodyTemplate()
            : base(new StubbleTemplateRenderer()) { }

        public static string DefaultTemplate { get; } =
@"{{#multipleChanges}}{{packageCount}} packages were updated in {{projectsUpdated}} project{{#multipleProjects}}s{{/multipleProjects}}:
{{#packages}}`{{Name}}`{{^Last}}, {{/Last}}{{/packages}}
<details>
<summary>Details of updated packages</summary>

{{/multipleChanges}}
{{#packages}}NuKeeper has generated a {{ActualChange}} update of `{{Name}}` to `{{Version}}`{{^MultipleUpdates}} from `{{FromVersion}}`{{/MultipleUpdates}}
{{#MultipleUpdates}}{{ProjectsUpdated}} versions of `{{Name}}` were found in use: {{#Updates}}`{{FromVersion}}`{{^Last}}, {{/Last}}{{/Updates}}{{/MultipleUpdates}}
{{#Publication}}`{{Name}} {{Version}}` was published at `{{Date}}`, {{Ago}}{{/Publication}}
{{#LatestVersion}}There is also a higher version, `{{Name}} {{Version}}`{{#Publication}} published at `{{Date}}`, {{Ago}}{{/Publication}}, but this was not applied as only `{{AllowedChange}}` version changes are allowed.
{{/LatestVersion}}
{{ProjectsUpdated}} project update{{#MultipleProjectsUpdated}}s{{/MultipleProjectsUpdated}}:
{{#Updates}}
Updated `{{SourceFilePath}}` to `{{Name}}` `{{ToVersion}}` from `{{FromVersion}}`
{{/Updates}}
{{#IsFromNuget}}

[{{Name}} {{Version}} on NuGet.org]({{Url}})
{{/IsFromNuget}}
{{/packages}}
{{#multipleChanges}}
</details>

{{/multipleChanges}}
{{#footer}}
{{WarningMessage}}
**NuKeeper**: {{NuKeeperUrl}}
{{/footer}}
";

        public string CustomTemplate { get; set; }

        public override string Value => CustomTemplate ?? DefaultTemplate;
    }
}
