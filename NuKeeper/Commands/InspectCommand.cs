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
        private readonly SettingsContainer _settings;
        private readonly LocalEngine _engine;

        public InspectCommand(SettingsContainer settings, LocalEngine engine, IConfigureLogLevel logger) :
            base(logger)
        {
            _settings = settings;
            _engine = engine;
        }

        protected override async Task<int> Run()
        {
            _settings.ModalSettings = new ModalSettings {Mode = RunMode.Inspect};
            _settings.UserSettings = new UserSettings
            {
                Directory = Path,
                AllowedChange = AllowedChange,
                NuGetSources = NuGetSources
            };

            await _engine.Run(_settings);
            return 0;
        }
    }
}
