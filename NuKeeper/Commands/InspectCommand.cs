using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Local;

namespace NuKeeper.Commands
{
    [Command(Description = "Checks projects existing locally for possible updates.")]
    internal class InspectCommand : LocalNuKeeperCommand
    {
        private readonly SettingsContainer _settings;
        private readonly LocalEngine _engine;

        public InspectCommand(SettingsContainer settings, LocalEngine engine)
        {
            _settings = settings;
            _engine = engine;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task<int> OnExecute()
        {
            _settings.ModalSettings = new ModalSettings {Mode = RunMode.Inspect};
            _settings.UserSettings = new UserSettings {Directory = Path, AllowedChange = AllowedChange, NuGetSources = NuGetSources};

            await _engine.Run(_settings);
            return 0;
        }
    }
}
