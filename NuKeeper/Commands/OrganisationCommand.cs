using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Collaboration;
using NuKeeper.Inspection.Logging;
using System.Threading.Tasks;

namespace NuKeeper.Commands
{
    [Command("org", "o", "organization", "organisation", Description = "Performs version checks and generates pull requests for all repositories in a github organisation or an Azure DevOps project.")]
    internal class OrganisationCommand : MultipleRepositoryCommand
    {
        [Argument(0, Name = "Organisation name", Description = "The organisation to scan.")]
        public string OrganisationName { get; set; }

        public OrganisationCommand(ICollaborationEngine engine, IConfigureLogger logger, IFileSettingsCache fileSettingsCache, ICollaborationFactory collaborationFactory)
            : base(engine, logger, fileSettingsCache, collaborationFactory)
        {
        }

        protected override async Task<ValidationResult> PopulateSettings(SettingsContainer settings)
        {
            var fileSettings = FileSettingsCache.GetSettings();
            ApiEndpoint = Concat.FirstValue(ApiEndpoint, fileSettings.Api, "https://api.github.com");

            var baseResult = await base.PopulateSettings(settings);
            if (!baseResult.IsSuccess)
            {
                return baseResult;
            }

            settings.SourceControlServerSettings.Scope = ServerScope.Organisation;
            settings.SourceControlServerSettings.OrganisationName = OrganisationName;
            return ValidationResult.Success;
        }
    }
}
