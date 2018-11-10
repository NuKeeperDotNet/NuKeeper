using LibGit2Sharp;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.DTOs;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Inspection.Files;
using System;
using System.Threading.Tasks;

namespace NuKeeper.Engine
{
    public class CollaborationEngine : ICollaborationEngine
    {
        private readonly ICollaborationFactory _collaborationFactory;
        private readonly ICollaborationPlatform _collaborationPlatform;
        private readonly IGitRepositoryEngine _repositoryEngine;
        private readonly IFolderFactory _folderFactory;
        private readonly INuKeeperLogger _logger;

        public CollaborationEngine(
            ICollaborationFactory collaborationFactory,
            ICollaborationPlatform collaborationPlatform,
            IGitRepositoryEngine repositoryEngine,
            IFolderFactory folderFactory,
            INuKeeperLogger logger)
        {
            _collaborationFactory = collaborationFactory;
            _collaborationPlatform = collaborationPlatform;
            _repositoryEngine = repositoryEngine;
            _folderFactory = folderFactory;
            _logger = logger;
        }

        public async Task<int> Run(SettingsContainer settings)
        {
            _logger.Detailed($"{Now()}: Started");
            _folderFactory.DeleteExistingTempDirs();
            _collaborationPlatform.Initialise(
                new AuthSettings(_collaborationFactory.Settings.BaseApiUrl, _collaborationFactory.Settings.Token)
            );

            var githubUser = await _collaborationPlatform.GetCurrentUser();
            var gitCreds = new UsernamePasswordCredentials
            {
                Username = githubUser.Login,
                Password = _collaborationFactory.Settings.Token
            };

            var userIdentity = GetUserIdentity(githubUser);

            var repositories = await _collaborationFactory.RepositoryDiscovery.GetRepositories(settings.SourceControlServerSettings);

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

        private Identity GetUserIdentity(User githubUser)
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
