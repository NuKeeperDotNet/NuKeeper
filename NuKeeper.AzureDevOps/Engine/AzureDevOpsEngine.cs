using LibGit2Sharp;
using NuKeeper.Abstract;
using NuKeeper.Abstract.Configuration;
using NuKeeper.Abstract.Engine;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.Logging;
using System;
using System.Security.Principal;
using System.Threading.Tasks;

namespace NuKeeper.AzureDevOps.Engine
{
    public class AzureDevOpsEngine
    {
        private readonly IAzureDevOpsClient _github;
        private readonly IAzureDevopsRepositoryDiscovery _repositoryDiscovery;
        private readonly IRepositoryEngine _repositoryEngine;
        private readonly IFolderFactory _folderFactory;
        private readonly INuKeeperLogger _logger;

        public AzureDevOpsEngine(
            IAzureDevOpsClient github,
            IAzureDevopsRepositoryDiscovery repositoryDiscovery,
            IRepositoryEngine repositoryEngine,
            IFolderFactory folderFactory,
            INuKeeperLogger logger)
        {
            _github = github;
            _repositoryDiscovery = repositoryDiscovery;
            _repositoryEngine = repositoryEngine;
            _folderFactory = folderFactory;
            _logger = logger;
        }

        public async Task<int> Run(SettingsContainer settings)
        {
            _logger.Detailed($"{Now()}: Started");
            _folderFactory.DeleteExistingTempDirs();
            var azureSettigns = new AzureDevopsAuthSettings(settings.AuthSettings.ApiBase, settings.AuthSettings.Token);
            _github.Initialise(azureSettigns);

            var githubUser = await _github.GetCurrentUser("id?") as IAccount;
            var gitCreds = new UsernamePasswordCredentials
            {
                Username = githubUser.Login,
                Password = settings.AuthSettings.Token
            };

            var userIdentity = GetUserIdentity(githubUser);

            var repositories = await _repositoryDiscovery.GetRepositories(_github, settings.SourceControlServerSettings);

            var reposUpdated = 0;

            foreach (var repository in repositories)
            {
                if (reposUpdated >= settings.UserSettings.MaxRepositoriesChanged)
                {
                    _logger.Detailed($"Reached max of {reposUpdated} repositories changed");
                    break;
                }

                var updatesInThisRepo = await _repositoryEngine.Run(repository,
                    gitCreds, userIdentity, settings);

                if (updatesInThisRepo > 0)
                {
                    reposUpdated++;
                }
            }

            if (reposUpdated > 1)
            {
                _logger.Detailed($"{reposUpdated} repositories were updated");
            }

            _logger.Detailed($"Done at {Now()}");
            return reposUpdated;
        }

        private Identity GetUserIdentity(IAccount githubUser)
        {
            if (string.IsNullOrWhiteSpace(githubUser?.Name))
            {
                _logger.Minimal("GitHub user name missing from profile, falling back to .gitconfig");
                return null;
            }
            if (string.IsNullOrWhiteSpace(githubUser?.Email))
            {
                _logger.Minimal("GitHub public email missing from profile, falling back to .gitconfig");
                return null;
            }

            return new Identity(githubUser.Name, githubUser.Email);
        }

        private static string Now()
        {
            return DateFormat.AsUtcIso8601(DateTimeOffset.Now);
        }
    }
}
