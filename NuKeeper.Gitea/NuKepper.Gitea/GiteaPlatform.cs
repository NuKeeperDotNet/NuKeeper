using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.Gitlab
{
    public class GiteaPlatform : ICollaborationPlatform
    {
        private readonly INuKeeperLogger _logger;
        private GiteaRestClient _client;

        public GiteaPlatform(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public void Initialise(AuthSettings settings)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = settings.ApiBase
            };

            _client = new GiteaRestClient(httpClient, settings.Token, _logger);
        }

        public async Task<User> GetCurrentUser()
        {
            var user = await _client.GetCurrentUser();
            return new User(user.Login, user.FullName, user.Email);
        }

        public async Task OpenPullRequest(ForkData target, PullRequestRequest request, IEnumerable<string> labels)
        {
            var projectName = target.Owner;
            var repositoryName = target.Name;

            var mergeRequest = new MergeRequest
            {
                Title = request.Title,
                SourceBranch = request.Head,
                Description = request.Body,
                TargetBranch = request.BaseRef,
                Id = $"{projectName}/{repositoryName}"
            };

            await _client.OpenMergeRequest(projectName, repositoryName, mergeRequest);
        }

        public Task<IReadOnlyList<Organization>> GetOrganizations()
        {
            _logger.Error("GitLab organizations have not yet been implemented.");
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string projectName)
        {
            _logger.Error("GitLab organizations have not yet been implemented.");
            throw new NotImplementedException();
        }

        public async Task<Repository> GetUserRepository(string userName, string repositoryName)
        {
            var repo = await _client.GetRepository(userName, repositoryName);

            return new Repository(repo.)
        }

        public Task<Repository> MakeUserFork(string owner, string repositoryName)
        {
            _logger.Error($"{ForkMode.PreferFork} has not yet been implemented for GitLab.");
            throw new NotImplementedException();
        }

        public async Task<bool> RepositoryBranchExists(string userName, string repositoryName, string branchName)
        {
            var result = await _client.CheckExistingBranch(userName, repositoryName, branchName);

            return result != null;
        }

        public Task<SearchCodeResult> Search(SearchCodeRequest search)
        {
            _logger.Error($"Search has not yet been implemented for GitLab.");
            throw new NotImplementedException();
        }
    }
}