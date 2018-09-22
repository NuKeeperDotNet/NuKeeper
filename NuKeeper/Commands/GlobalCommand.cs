using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Commands
{
    [Command(Description = "Performs version checks and generates pull requests for all repositories the provided token can access.")]
    internal class GlobalCommand : MultipleRepositoryCommand
    {
        public GlobalCommand(IGitHubEngine engine, IConfigureLogger logger, IFileSettingsCache fileSettingsCache)
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

            var apiHost = settings.GithubAuthSettings.ApiBase.Host;
            if (apiHost.EndsWith("github.com"))
            {
                return ValidationResult.Failure("Global mode must not use public github");
            }

            return ValidationResult.Success;
        }
    }
}
