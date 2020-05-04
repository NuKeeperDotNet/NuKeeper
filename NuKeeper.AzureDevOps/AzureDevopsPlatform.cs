using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsPlatform : ICollaborationPlatform
    {
        private readonly INuKeeperLogger _logger;
        private AzureDevOpsRestClient _client;

        public AzureDevOpsPlatform(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public void Initialise(AuthSettings settings)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = settings.ApiBase
            };
            _client = new AzureDevOpsRestClient(httpClient, _logger, settings.Token);
        }

        public Task<User> GetCurrentUser()
        {
            return Task.FromResult(new User("user@email.com", "", ""));
        }

        public async Task OpenPullRequest(ForkData target, PullRequestRequest request, IEnumerable<string> labels)
        {
            var repos = await _client.GetGitRepositories(target.Owner);
            var repo = repos.Single(x => x.name == target.Name);

            var req = new PRRequest
            {
                title = request.Title,
                sourceRefName = $"refs/heads/{request.Head}",
                description = request.Body,
                targetRefName = $"refs/heads/{request.BaseRef}",
                completionOptions = new GitPullRequestCompletionOptions
                {
                    deleteSourceBranch = request.DeleteBranchAfterMerge
                },
            };

            var pullRequest = await _client.CreatePullRequest(req, target.Owner, repo.id);

            if (request.SetAutoComplete)
            {
                await _client.SetAutoComplete(new PRRequest()
                    {
                        autoCompleteSetBy = new Creator()
                        {
                            id = pullRequest.CreatedBy.id
                        }
                    }, target.Owner,
                    repo.id,
                    pullRequest.PullRequestId);
            }

            foreach (var label in labels)
            {
                await _client.CreatePullRequestLabel(new LabelRequest { name = label }, target.Owner, repo.id, pullRequest.PullRequestId);
            }
        }

        public async Task<IReadOnlyList<Organization>> GetOrganizations()
        {
            var projects = await _client.GetProjects();
            return projects
                .Select(project => new Organization(project.name))
                .ToList();
        }

        public async Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string projectName)
        {
            var repos = await _client.GetGitRepositories(projectName);
            return repos.Select(x =>
                    new Repository(x.name, false,
                        new UserPermissions(true, true, true),
                        new Uri(x.remoteUrl),
                        null, false, null))
                .ToList();
        }

        public async Task<Repository> GetUserRepository(string projectName, string repositoryName)
        {
            var repos = await GetRepositoriesForOrganisation(projectName);
            return repos.Single(x => x.Name == repositoryName);
        }

        public Task<Repository> MakeUserFork(string owner, string repositoryName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RepositoryBranchExists(string projectName, string repositoryName, string branchName)
        {
            var repos = await _client.GetGitRepositories(projectName);
            var repo = repos.Single(x => x.name == repositoryName);
            var refs = await _client.GetRepositoryRefs(projectName, repo.id);
            var count = refs.Count(x => x.name.EndsWith(branchName, StringComparison.OrdinalIgnoreCase));
            if (count > 0)
            {
                _logger.Detailed($"Branch found for {projectName} / {repositoryName} / {branchName}");
                return true;
            }

            _logger.Detailed($"No branch found for {projectName} / {repositoryName} / {branchName}");
            return false;
        }

        public Task<SearchCodeResult> Search(SearchCodeRequest search)
        {
            throw new NotImplementedException();
        }
    }
}
