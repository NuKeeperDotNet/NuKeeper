using System;
using System.Threading.Tasks;
using NuKeeper.Github;
using NuKeeper.Logging;

namespace NuKeeper.Engine
{

    public class ForkFinder: IForkFinder
    {
        private readonly IGithub _github;
        private readonly INuKeeperLogger _logger;

        public ForkFinder(IGithub github,INuKeeperLogger logger)
        {
            _github = github;
            _logger = logger;
        }

        public async Task<ForkData> PushFork(string userName, string repositoryName, ForkData fallbackFork)
        {
            var userFork = await _github.GetUserRepository(userName, repositoryName);
            if (userFork != null)
            {
                _logger.Info($"Found push fork for user {userName} at {userFork.HtmlUrl}");
                return new ForkData(new Uri(userFork.HtmlUrl), userFork.Owner.Login, userFork.Name);
            }

            // for now we pull and push from the same place as a fallback
            _logger.Info($"Using fallback push fork for user {fallbackFork.Owner} at {fallbackFork.Uri}");
            return fallbackFork;
        }
    }
}
