using System;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Git;
using NuKeeper.Github;
using NuKeeper.NuGet.Api;

namespace NuKeeper
{
    public class Runner
    {
        private readonly IGithubRepositoryDiscovery _repositoryDiscovery;
        private readonly IPackageUpdatesLookup _updatesLookup;
        private readonly IPackageUpdateSelection _updateSelection;
        private readonly IGithub _github;
        private readonly string _githubToken;

        public Runner(
            IGithubRepositoryDiscovery repositoryDiscovery, 
            IPackageUpdatesLookup updatesLookup, 
            IPackageUpdateSelection updateSelection, 
            IGithub github,
            Settings settings)
        {
            _repositoryDiscovery = repositoryDiscovery;
            _updatesLookup = updatesLookup;
            _updateSelection = updateSelection;
            _github = github;
            _githubToken = settings.GithubToken;
        }

        public async Task Run(string tempDir)
        {
            var githubUser = await _github.GetCurrentUser();
            Console.WriteLine($"Read github user '{githubUser}'");

            var git = new LibGit2SharpDriver(tempDir, githubUser, _githubToken);

            var repositories = await _repositoryDiscovery.GetRepositories();

            foreach (var repository in repositories)
            {
                try
                {
                    var repositoryUpdater = new RepositoryUpdater(_updatesLookup, _github, git, tempDir, _updateSelection, repository);
                    await repositoryUpdater.Run();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Repo failed {e.GetType().Name}: {e.Message}");
                }
            }
        }
    }
}
