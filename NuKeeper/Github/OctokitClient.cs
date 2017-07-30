
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Github.Models;
using Octokit;

namespace NuKeeper.Github
{
    public class OctokitClient : IGithub
    {
        private readonly IGitHubClient _client;
        
        public OctokitClient(Settings settings)
        {
            var client = new GitHubClient(new ProductHeaderValue("NuKeeper"), settings.GithubApiBase);
            client.Credentials = new Credentials(settings.GithubToken);
            _client = client;
        }

        public async Task<string> GetCurrentUser()
        {
            return (await _client.User.Current()).Login;
        }

        public async Task<IReadOnlyList<GithubRepository>> GetRepositoriesForOrganisation(string organisationName)
        {
            var organisation = await _client.Organization.Get(organisationName);
            var results = await _client.Repository.GetAllForOrg(organisationName);
            
            return results.Select(r => new GithubRepository
            {
                Name = r.Name,
                Owner = r.Owner.Login,
                HtmlUrl = r.HtmlUrl    
            }).ToList().AsReadOnly();
        }

        public async Task OpenPullRequest(OpenPullRequestRequest request)
        {
            var newPullRequest = new NewPullRequest(request.Data.Title, request.Data.Head, request.Data.Base);
            newPullRequest.Body = request.Data.Body;
            await _client.PullRequest.Create(request.RepositoryOwner, request.RepositoryOwner, newPullRequest);
        }
    }
}