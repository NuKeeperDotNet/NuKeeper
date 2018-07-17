using NuKeeper.Configuration;
using NuKeeper.Github;
using NuKeeper.Inspection.Logging;

namespace NuKeeper
{
    public class OctokitClientCreator : ICreate<IGithub>
    {
        private readonly INuKeeperLogger _logger;

        public OctokitClientCreator(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public IGithub Create(SettingsContainer settings)
        {
            return new OctokitClient(settings.GithubAuthSettings, _logger);
        }
    }
}
