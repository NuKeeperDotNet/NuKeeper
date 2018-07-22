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
        private readonly GithubEngine _engine;

        [Argument(0, Name = "GitHub organisation name", Description = "The organisation to scan.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string GithubOrganisationName { get; }

        public OrganisationCommand(GithubEngine engine, IConfigureLogLevel logger) : base(logger)
        {
            _engine = engine;
        }

        protected override async Task<int> Run(SettingsContainer settings)
        {
            await base.Run(settings);
            settings.ModalSettings.Mode = RunMode.Organisation;
            settings.ModalSettings.OrganisationName = GithubOrganisationName;

            await _engine.Run(settings);
            return 0;
        }
    }
}
