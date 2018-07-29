using NuKeeper.Configuration;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Creators
{
    public class OctokitClientCreator : ICreate<IGitHub>
    {
        private readonly INuKeeperLogger _logger;

        public OctokitClientCreator(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public IGitHub Create(SettingsContainer settings)
        {
            return new OctokitClient(settings.GithubAuthSettings, _logger);
        }
    }
}
