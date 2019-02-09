using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Inspection.Logging;
using NuKeeper.Local;

namespace NuKeeper.Commands
{
    [Command("inspect", "i", Description = "Checks projects existing locally for possible updates.")]
    internal class InspectCommand : LocalNuKeeperCommand
    {
        private readonly ILocalEngine _engine;

        public InspectCommand(ILocalEngine engine, IConfigureLogger logger, IFileSettingsCache fileSettingsCache) :
            base(logger, fileSettingsCache)
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

            return ValidationResult.Success;
        }

        protected override async Task<int> Run(SettingsContainer settings)
        {
            await _engine.Run(settings, false);
            return 0;
        }
    }
}
