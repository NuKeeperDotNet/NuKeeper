using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;
using NuKeeper.Local;

namespace NuKeeper.Commands
{
    [Command(Description = "Applies first relevant update to a local project.")]
    internal class UpdateCommand : LocalNuKeeperCommand {
        private readonly LocalEngine _engine;

        public UpdateCommand(LocalEngine engine, IConfigureLogLevel logger)
            : base(logger)
        {
            _engine = engine;
        }

        protected override async Task<int> Run(SettingsContainer settings)
        {
            await base.Run(settings);
            settings.ModalSettings.Mode = RunMode.Update;

            await _engine.Run(settings);
            return 0;
        }
    }
}
