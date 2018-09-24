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
        [Option(CommandOptionType.SingleValue, ShortName = "m", LongName = "maxupdate",
            Description = "Maximum number of package updates to make. Defaults to 1.")]
        protected int MaxPackageUpdates { get; } = 1;

        private readonly ILocalEngine _engine;

        public UpdateCommand(ILocalEngine engine, IConfigureLogger logger, IFileSettingsCache fileSettingsCache)
            : base(logger, fileSettingsCache)
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

            settings.PackageFilters.MaxPackageUpdates = MaxPackageUpdates;

            return ValidationResult.Success;
        }

        protected override async Task<int> Run(SettingsContainer settings)
        {
            await _engine.Run(settings, true);
            return 0;
        }
    }
}
