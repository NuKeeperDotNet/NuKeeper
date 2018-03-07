using System.Threading.Tasks;
using LibGit2Sharp;
using NuKeeper.Configuration;
using NuKeeper.Github;
using NuKeeper.Files;

namespace NuKeeper.Engine
{
    public class GithubEngine
    {
        private readonly IGithub _github;
        private readonly IGithubRepositoryDiscovery _repositoryDiscovery;
        private readonly IGithubRepositoryEngine _repositoryEngine;
        private readonly string _githubToken;
        private readonly IFolderFactory _folderFactory;

        public GithubEngine(
            IGithub github,
            IGithubRepositoryDiscovery repositoryDiscovery,
            IGithubRepositoryEngine repositoryEngine,
            GithubAuthSettings settings,
            IFolderFactory folderFactory)
        {
            _github = github;
            _repositoryDiscovery = repositoryDiscovery;
            _repositoryEngine = repositoryEngine;
            _githubToken = settings.Token;
            _folderFactory = folderFactory;
        }

        public async Task Run()
        {
            _folderFactory.DeleteExistingTempDirs();

            var githubUser = await _github.GetCurrentUser();
            var gitCreds = new UsernamePasswordCredentials
            {
                Username = githubUser.Login,
                Password = _githubToken
            };
            var userIdentity = new Identity(githubUser.Name, githubUser.Email);

            var repositories = await _repositoryDiscovery.GetRepositories();

            foreach (var repository in repositories)
            {
                await _repositoryEngine.Run(repository, gitCreds, userIdentity);
            }
        }
    }
}
