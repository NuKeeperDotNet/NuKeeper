using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

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
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

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

        public async Task<bool> PullRequestExists(ForkData target, string headBranch, string baseBranch)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var repos = await _client.GetGitRepositories(target.Owner);
            var repo = repos.Single(x => x.name == target.Name);

            var result = await _client.GetPullRequests(
                target.Owner,
                repo.id,
                $"refs/heads/{headBranch}",
                $"refs/heads/{baseBranch}");

            return result.Any();
        }

        public async Task OpenPullRequest(ForkData target, PullRequestRequest request, IEnumerable<string> labels)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (labels == null)
            {
                throw new ArgumentNullException(nameof(labels));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

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
                }
            };

            var pullRequest = await _client.CreatePullRequest(req, target.Owner, repo.id);

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

        public async Task<SearchCodeResult> Search(SearchCodeRequest searchRequest)
        {
            if (searchRequest == null)
            {
                throw new ArgumentNullException(nameof(searchRequest));
            }

            var totalCount = 0;
            var repositoryFileNames = new List<string>();
            foreach (var repo in searchRequest.Repos)
            {
                repositoryFileNames.AddRange(await _client.GetGitRepositoryFileNames(repo.Owner, repo.Name));
            }

            var searchStrings = searchRequest.Term
                .Replace("\"", string.Empty)
                .Split(new[] { "OR" }, StringSplitOptions.None);

            foreach (var searchString in searchStrings)
            {
                totalCount += repositoryFileNames.FindAll(x => x.EndsWith(searchString.Trim(), StringComparison.InvariantCultureIgnoreCase)).Count;
            }

            return new SearchCodeResult(totalCount);
        }
    }
}
