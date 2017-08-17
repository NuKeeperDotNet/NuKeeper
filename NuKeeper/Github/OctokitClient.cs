using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Logging;
using Octokit;

namespace NuKeeper.Github
{
    public class OctokitClient : IGithub
    {
        private readonly INuKeeperLogger _logger;
        private readonly IGitHubClient _client;
        private readonly Uri _apiBase;

        public OctokitClient(Settings settings, INuKeeperLogger logger)
        {
            _logger = logger;
            _apiBase = settings.GithubApiBase;

            _client = new GitHubClient(new ProductHeaderValue("NuKeeper"), _apiBase)
            {
                Credentials = new Credentials(settings.GithubToken)
            };
        }

        public async Task<string> GetCurrentUser()
        {
            var user = await _client.User.Current();
            var userLogin = user?.Login;
            _logger.Verbose($"Read github user '{userLogin}'");
            return userLogin;
        }

        public async Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string organisationName)
        {
            var results = await _client.Repository.GetAllForOrg(organisationName);
            var resultList = results.Where(r => r.Permissions.Pull && r.Permissions.Push).ToList().AsReadOnly();
            _logger.Verbose($"Read {resultList.Count} repos for org '{organisationName}'");
            return resultList;
        }

        public async Task OpenPullRequest(string repositoryOwner, string repositoryName, NewPullRequest request)
        {
            _logger.Info($"Making PR on '{_apiBase} {repositoryOwner}/{repositoryName}'");
            _logger.Verbose($"PR title: {request.Title}");
            await _client.PullRequest.Create(repositoryOwner, repositoryName, request);
        }
    }
}