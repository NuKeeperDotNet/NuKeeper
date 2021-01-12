using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Gitlab.Model;

using User = NuKeeper.Abstractions.CollaborationModels.User;

namespace NuKeeper.Gitlab
{
    public class GitlabPlatform : ICollaborationPlatform
    {
        private readonly INuKeeperLogger _logger;
        private readonly IHttpClientFactory _clientFactory;
        private GitlabRestClient _client;

        public GitlabPlatform(INuKeeperLogger logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        public void Initialise(AuthSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _client = new GitlabRestClient(_clientFactory, settings.Token, _logger, settings.ApiBase);
        }

        public async Task<User> GetCurrentUser()
        {
            var user = await _client.GetCurrentUser();

            return new User(user.UserName, user.Name, user.Email);
        }

        public async Task<bool> PullRequestExists(ForkData target, string headBranch, string baseBranch)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var projectName = target.Owner;
            var repositoryName = target.Name;

            var result = await _client.GetMergeRequests(projectName, repositoryName, headBranch, baseBranch);

            return result.Any();
        }

        public async Task OpenPullRequest(ForkData target, PullRequestRequest request, IEnumerable<string> labels)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var projectName = target.Owner;
            var repositoryName = target.Name;

            var mergeRequest = new MergeRequest
            {
                Title = request.Title,
                SourceBranch = request.Head,
                Description = request.Body,
                TargetBranch = request.BaseRef,
                Id = $"{projectName}/{repositoryName}",
                RemoveSourceBranch = request.DeleteBranchAfterMerge,
                Labels = labels.ToList()
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
            var project = await _client.GetProject(userName, repositoryName);

            return new Repository(project.Name, project.Archived,
                new UserPermissions(true, true, true),
                project.HttpUrlToRepo,
                null, false, null);
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

        public Task<int> GetNumberOfOpenPullRequests(string projectName, string repositoryName)
        {
            return Task.FromResult(0);
        }
    }
}
