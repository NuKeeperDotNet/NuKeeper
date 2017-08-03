using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using Octokit;

namespace NuKeeper.Github
{
    public class OctokitClient : IGithub
    {
        private readonly IGitHubClient _client;
        
        public OctokitClient(Settings settings)
        {
            _client = new GitHubClient(new ProductHeaderValue("NuKeeper"), settings.GithubApiBase)
            {
                Credentials = new Credentials(settings.GithubToken)
            };
        }

        public async Task<string> GetCurrentUser()
        {
            return (await _client.User.Current()).Login;
        }

        public async Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string organisationName)
        {
            var results = await _client.Repository.GetAllForOrg(organisationName);
            
            return results.ToList().AsReadOnly();
        }

        public async Task OpenPullRequest(string repositoryOwner, string repositoryName, NewPullRequest request)
        {
            await _client.PullRequest.Create(repositoryOwner, repositoryName, request);
        }
    }
}