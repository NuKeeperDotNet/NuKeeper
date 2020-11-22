using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Engine;
using NuKeeper.Inspection.Files;
using System;
using System.Threading.Tasks;

namespace NuKeeper.Collaboration
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
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _logger.Detailed($"{Now()}: Started");
            _folderFactory.DeleteExistingTempDirs();

            var user = await _collaborationFactory.CollaborationPlatform.GetCurrentUser();
            var credentials = new GitUsernamePasswordCredentials
            {
                Username = user.Login,
                Password = _collaborationFactory.Settings.Token
            };

            var repositories = await _collaborationFactory.RepositoryDiscovery.GetRepositories(settings.SourceControlServerSettings);

            var reposUpdated = 0;
            (bool Happened, Exception Value) unhandledEx = (false, null);

            foreach (var repository in repositories)
            {
                if (reposUpdated >= settings.UserSettings.MaxRepositoriesChanged)
                {
                    _logger.Detailed($"Reached max of {reposUpdated} repositories changed");
                    break;
                }
                try
                {

                    var updatesInThisRepo = await _repositoryEngine.Run(repository,
                        credentials, settings, user);

                    if (updatesInThisRepo > 0)
                    {
                        reposUpdated++;
                    }
                }
#pragma warning disable CA1031
                catch (Exception ex)
#pragma warning restore CA1031
                {
                    _logger.Error($"Failed on repo {repository.RepositoryName}", ex);
                    SetOrUpdateUnhandledException(ref unhandledEx, ex);
                }
            }

            if (reposUpdated > 1)
            {
                _logger.Detailed($"{reposUpdated} repositories were updated");
            }

            _logger.Detailed($"Done at {Now()}");

            ThrowIfUnhandledException(unhandledEx);

            return reposUpdated;
        }

        private static string Now()
        {
            return DateFormat.AsUtcIso8601(DateTimeOffset.Now);
        }

        private static void SetOrUpdateUnhandledException(
            ref (bool Happened, Exception Value) unhandledEx,
            Exception ex
        )
        {
            unhandledEx.Happened = true;
            if (unhandledEx.Value == null)
            {
                unhandledEx.Value = ex;
            }
            else
            {
                unhandledEx.Value = new AggregateException(unhandledEx.Value, ex);
            }
        }

        private static void ThrowIfUnhandledException(
            (bool Happened, Exception Value) unhandledEx
        )
        {
            if (unhandledEx.Happened)
            {
                var exception = unhandledEx.Value;
                if (exception is AggregateException aggregateException)
                {
                    exception = aggregateException.Flatten();
                }
                throw new NuKeeperException("One or multiple repositories failed to update.", exception);
            }
        }
    }
}
