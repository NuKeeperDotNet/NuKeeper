using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;
using NuKeeper.Local;

namespace NuKeeper.Commands
{
    [Command(Description = "Checks projects existing locally for possible updates.")]
    internal class InspectCommand : LocalNuKeeperCommand
    {
        private readonly LocalEngine _engine;

        public InspectCommand(LocalEngine engine, IConfigureLogLevel logger) :
            base(logger)
        {
            _engine = engine;
        }

        protected override void PopulateSettings(SettingsContainer settings)
        {
            base.PopulateSettings(settings);
            settings.ModalSettings.Mode = RunMode.Inspect;
        }

        protected override async Task<int> Run(SettingsContainer settings)
        {
            await _engine.Run(settings);
            return 0;
        }
    }
}
