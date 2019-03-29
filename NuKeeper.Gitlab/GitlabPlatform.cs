using System;
using System.Collections.Generic;
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
        private GitlabRestClient _client;
        private AuthSettings _settings;

        public GitlabPlatform(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public void Initialise(AuthSettings settings)
        {
            _settings = settings;

            var httpClient = new HttpClient
            {
                BaseAddress = settings.ApiBase
            };
            _client = new GitlabRestClient(httpClient, settings.Token, _logger);
        }

        public async Task<User> GetCurrentUser()
        {
            var user = await _client.GetCurrentUser().ConfigureAwait(false);

            return new User(user.UserName, user.Name, user.Email);
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

            await _client.OpenMergeRequest(projectName, repositoryName, mergeRequest).ConfigureAwait(false);
        }

        public Task<IReadOnlyList<Organization>> GetOrganizations()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string projectName)
        {
            throw new NotImplementedException();
        }

        public async Task<Repository> GetUserRepository(string userName, string repositoryName)
        {
            var project = await _client.GetProject(userName, repositoryName).ConfigureAwait(false);

            return new Repository(project.Name, project.Archived,
                new UserPermissions(true, true, true),
                project.HttpUrlToRepo,
                null, false, null);
        }

        public Task<Repository> MakeUserFork(string owner, string repositoryName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RepositoryBranchExists(string userName, string repositoryName, string branchName)
        {
            var result = await _client.CheckExistingBranch(userName, repositoryName, branchName).ConfigureAwait(false);

            return result != null;
        }

        public Task<SearchCodeResult> Search(SearchCodeRequest search)
        {
            throw new NotImplementedException();
        }
    }
}
