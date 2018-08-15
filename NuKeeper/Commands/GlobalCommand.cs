using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Commands
{
    [Command(Description = "Performs version checks and generates pull requests for all repositories the provided token can access.")]
    internal class GlobalCommand : GitHubNuKeeperCommand
    {
        private readonly GitHubEngine _engine;

        public GlobalCommand(GitHubEngine engine, IConfigureLogLevel logger) : base(logger)
        {
            _engine = engine;
        }

        protected override ValidationResult PopulateSettings(SettingsContainer settings)
        {
            var baseResult = base.PopulateSettings(settings);
            if (!baseResult.IsSuccess)
            {
                return baseResult;
            }

            if (settings.UserSettings.PackageIncludes == null)
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

        protected override async Task<int> Run(SettingsContainer settings)
        {
            await _engine.Run(GithubScope.Global, settings);
            return 0;
        }

    }
}
