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
        private readonly GithubEngine _engine;

        public GlobalCommand(GithubEngine engine, IConfigureLogLevel logger) : base(logger)
        {
            _engine = engine;
        }

        protected override async Task<int> Run(SettingsContainer settings)
        {
            await base.Run(settings);
            settings.ModalSettings.Mode = RunMode.Global;

            await _engine.Run(settings);
            return 0;
        }
    }
}
