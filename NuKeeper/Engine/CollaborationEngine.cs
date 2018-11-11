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
        private readonly IGitRepositoryEngine _repositoryEngine;
        private readonly IFolderFactory _folderFactory;
        private readonly INuKeeperLogger _logger;

        public CollaborationEngine(
            ICollaborationFactory collaborationFactory,
            IGitRepositoryEngine repositoryEngine,
            IFolderFactory folderFactory,
            INuKeeperLogger logger)
        {
            _collaborationFactory = collaborationFactory;
            _repositoryEngine = repositoryEngine;
            _folderFactory = folderFactory;
            _logger = logger;
        }

        public async Task<int> Run(SettingsContainer settings)
        {
            _logger.Detailed($"{Now()}: Started");
            _folderFactory.DeleteExistingTempDirs();

            var user = await _collaborationFactory.CollaborationPlatform.GetCurrentUser();
            var gitCreds = new UsernamePasswordCredentials
            {
                Username = user.Login,
                Password = _collaborationFactory.Settings.Token
            };

            var userIdentity = GetUserIdentity(user);

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

        private Identity GetUserIdentity(User user)
        {
            if (string.IsNullOrWhiteSpace(user?.Name))
            {
                _logger.Minimal("User name missing from profile, falling back to .gitconfig");
                return null;
            }
            if (string.IsNullOrWhiteSpace(user?.Email))
            {
                _logger.Minimal("Email missing from profile, falling back to .gitconfig");
                return null;
            }

            return new Identity(user.Name, user.Email);
        }

        private static string Now()
        {
            return DateFormat.AsUtcIso8601(DateTimeOffset.Now);
        }
    }
}
