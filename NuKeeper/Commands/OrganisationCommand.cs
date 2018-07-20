using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;

namespace NuKeeper
{
    internal class OrganisationCommand : GitHubNuKeeperCommand
    {
        public OrganisationCommand(IConfigureLogLevel logger) : base(logger)
        {
        }

        protected override async Task<int> Run(SettingsContainer settings)
        {
            await Task.Delay(1);
            return 1;
        }
    }
}
