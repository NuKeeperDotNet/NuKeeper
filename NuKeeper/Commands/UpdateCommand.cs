using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;
using NuKeeper.Local;

namespace NuKeeper.Commands
{
    [Command(Description = "Applies relevant updates to a local project.")]
    internal class UpdateCommand : LocalNuKeeperCommand
    {
        [Option(CommandOptionType.SingleValue, ShortName = "m", LongName = "maxupdates",
            Description = "Maximum number of package updates to make. Defaults to 1.")]
        protected int MaxPackageUpdates { get; } = 1;

        private readonly LocalEngine _engine;

        public UpdateCommand(LocalEngine engine, IConfigureLogLevel logger)
            : base(logger)
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

            settings.ModalSettings.Mode = RunMode.Update;
            settings.UserSettings.MaxPackageUpdates = MaxPackageUpdates;

            return ValidationResult.Success;
        }

        protected override async Task<int> Run(SettingsContainer settings)
        {
            await _engine.Run(settings);
            return 0;
        }
    }
}
