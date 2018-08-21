using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Commands
{
    [Command(Description = "Performs version checks and generates pull requests for all repositories in a github organisation.")]
    internal class OrganisationCommand : GitHubNuKeeperCommand
    {
        private readonly GitHubEngine _engine;

        [Argument(0, Name = "GitHub organisation name", Description = "The organisation to scan.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string GithubOrganisationName { get; }

        public OrganisationCommand(GitHubEngine engine, IConfigureLogLevel logger) : base(logger)
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

            settings.SourceControlServerSettings.OrganisationName = GithubOrganisationName;
            return ValidationResult.Success;
        }

        protected override async Task<int> Run(SettingsContainer settings)
        {
            await _engine.Run(ServerScope.Organisation, settings);
            return 0;
        }
    }
}
