using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Logging;
using Octokit;

namespace NuKeeper.Github
{
    public class OctokitClient : IGithub
    {
        private readonly INuKeeperLogger _logger;
        private readonly IGitHubClient _client;
        private readonly Uri _apiBase;

        public OctokitClient(GithubAuthSettings settings, INuKeeperLogger logger)
        {
            _logger = logger;
            _apiBase = settings.ApiBase;

            _client = new GitHubClient(new ProductHeaderValue("NuKeeper"), _apiBase)
            {
                Credentials = new Credentials(settings.Token)
            };
        }

        public async Task<Account> GetCurrentUser()
        {
            var user = await _client.User.Current();
            var userLogin = user?.Login;
            _logger.Verbose($"Read github user '{userLogin}'");
            return user;
        }

        public async Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string organisationName)
        {
            var repos = await _client.Repository.GetAllForOrg(organisationName);
            _logger.Info($"Read {repos.Count} repos for org '{organisationName}'");
            return repos;
        }

        public async Task<Repository> GetUserRepository(string userName, string repositoryName)
        {
            _logger.Verbose($"Looking for user fork for {userName}/{repositoryName}");
            try
            {
                var result = await _client.Repository.Get(userName, repositoryName);
                _logger.Info($"User fork found at {result.GitUrl} for {result.Owner.Login}");
                return result;
            }
            catch (NotFoundException)
            {
                _logger.Verbose("User fork not found");
                return null;
            }
        }

        public async Task<Repository> MakeUserFork(string owner, string repositoryName)
        {
            _logger.Verbose($"Making user fork for {repositoryName}");
            try
            {
                var result = await _client.Repository.Forks.Create(owner, repositoryName, new NewRepositoryFork());
                _logger.Info($"User fork created at {result.GitUrl} for {result.Owner.Login}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("User fork not created", ex);
                return null;
            }
        }

        public async Task<Branch> GetRepositoryBranch(string userName, string repositoryName, string branchName)
        {
            try
            {
                return await _client.Repository.Branch.Get(userName, repositoryName, branchName);
            }
            catch (NotFoundException)
            {
                return null;
            }
        }

        public async Task OpenPullRequest(ForkData target, NewPullRequest request)
        {
            _logger.Info($"Making PR onto '{_apiBase} {target.Owner}/{target.Name} from {request.Head}");
            _logger.Verbose($"PR title: {request.Title}");
            await _client.PullRequest.Create(target.Owner, target.Name, request);
        }
    }
}
