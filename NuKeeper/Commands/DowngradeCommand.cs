using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Inspection.Logging;
using NuKeeper.Local;
using System.Threading.Tasks;

namespace NuKeeper.Commands
{
    [Command("downgrade", Description = "Applies relevant downgrades to a local project.")]
    internal class DowngradeCommand : LocalNuKeeperCommand
    {
        [Option(CommandOptionType.SingleValue, ShortName = "m", LongName = "maxpackageupdates",
         Description = "Maximum number of package downgrades to make. Defaults to 1.")]
        public int? MaxPackageDowngrades { get; set; }

        private readonly ILocalEngine _engine;

        public DowngradeCommand(ILocalEngine engine, IConfigureLogger logger, IFileSettingsCache fileSettingsCache)
        : base(logger, fileSettingsCache)
        {
            _engine = engine;
        }

        protected override async Task<ValidationResult> PopulateSettings(SettingsContainer settings)
        {
            var baseResult = await base.PopulateSettings(settings);
            if (!baseResult.IsSuccess)
            {
                return baseResult;
            }

            const int defaultMaxPackageUpdates = 1;
            var fileSettings = FileSettingsCache.GetSettings();

            var maxDowngrades = Concat.FirstValue(
                MaxPackageDowngrades,
                fileSettings.MaxPackageUpdates,
                defaultMaxPackageUpdates);

            if (maxDowngrades < 1)
            {
                return ValidationResult.Failure($"Max package downgrades of {maxDowngrades} is not valid");
            }

            settings.PackageFilters.MaxPackageUpdates = maxDowngrades;
            return ValidationResult.Success;
        }

        protected override async Task<int> Run(SettingsContainer settings)
        {
            await _engine.RunDowngrade(settings, true);
            return 0;
        }

    }
}
