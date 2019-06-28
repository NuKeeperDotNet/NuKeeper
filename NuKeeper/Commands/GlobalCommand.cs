using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Collaboration;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Commands
{
    [Command("global", Description = "Performs version checks and generates pull requests for all repositories the provided token can access.")]
    internal class GlobalCommand : MultipleRepositoryCommand
    {
        public GlobalCommand(ICollaborationEngine engine, IConfigureLogger logger, IFileSettingsCache fileSettingsCache, ICollaborationFactory collaborationFactory)
            : base(engine, logger, fileSettingsCache, collaborationFactory)
        {
        }

        protected override async Task<ValidationResult> PopulateSettings(SettingsContainer settings)
        {
            var baseResult = await base.PopulateSettings(settings);
            if (!baseResult.IsSuccess)
            {
                return baseResult;
            }

            settings.SourceControlServerSettings.Scope = ServerScope.Global;

            if (settings.PackageFilters.Includes == null)
            {
                return ValidationResult.Failure("Global mode must have an include regex");
            }

            var apiHost = CollaborationFactory.Settings.BaseApiUrl.Host;
            if (apiHost.EndsWith("github.com", StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.Failure("Global mode must not use public github");
            }

            return ValidationResult.Success;
        }
    }
}
