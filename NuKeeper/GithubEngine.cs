using System;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Git;
using NuKeeper.Github;
using NuKeeper.Logging;
using NuKeeper.NuGet.Api;

namespace NuKeeper
{
    public class GithubEngine
    {
        private readonly IGithubRepositoryDiscovery _repositoryDiscovery;
        private readonly IPackageUpdatesLookup _updatesLookup;
        private readonly IPackageUpdateSelection _updateSelection;
        private readonly IGithub _github;
        private readonly INuKeeperLogger _logger;
        private readonly string _githubToken;

        public GithubEngine(
            IGithubRepositoryDiscovery repositoryDiscovery, 
            IPackageUpdatesLookup updatesLookup, 
            IPackageUpdateSelection updateSelection, 
            IGithub github,
            INuKeeperLogger logger,
            Settings settings)
        {
            _repositoryDiscovery = repositoryDiscovery;
            _updatesLookup = updatesLookup;
            _updateSelection = updateSelection;
            _github = github;
            _logger = logger;
            _githubToken = settings.GithubToken;
        }

        public async Task Run(string tempDir)
        {
            var githubUser = await _github.GetCurrentUser();
            var git = new LibGit2SharpDriver(_logger, tempDir, githubUser, _githubToken);

            var repositories = await _repositoryDiscovery.GetRepositories();

            foreach (var repository in repositories)
            {
                try
                {
                    var repositoryUpdater = new RepositoryUpdater(
                        _updatesLookup, _github, git, _logger,
                        tempDir, _updateSelection, repository);

                    await repositoryUpdater.Run();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed on repo {repository.RepositoryName}", ex);
                }
            }
        }
    }
}
