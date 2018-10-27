using System;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Abstract;
using NuKeeper.Abstract.Configuration;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Commands
{
    [Command(Description = "Performs version checks and generates pull requests for all repositories the provided token can access.")]
    internal class GlobalCommand : MultipleRepositoryCommand
    {
        public GlobalCommand(IEngine engine, IConfigureLogger logger, IFileSettingsCache fileSettingsCache)
            : base(engine, logger, fileSettingsCache)
        {
        }

        protected override ValidationResult PopulateSettings(SettingsContainer settings)
        {
            var baseResult = base.PopulateSettings(settings);
            if (!baseResult.IsSuccess)
            {
                return baseResult;
            }

            settings.SourceControlServerSettings.Scope = ServerScope.Global;

            if (settings.PackageFilters.Includes == null)
            {
                return ValidationResult.Failure("Global mode must have an include regex");
            }

            var apiHost = settings.AuthSettings.ApiBase.Host;
            if (apiHost.EndsWith("github.com", StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.Failure("Global mode must not use public github");
            }

            return ValidationResult.Success;
        }
    }
}
