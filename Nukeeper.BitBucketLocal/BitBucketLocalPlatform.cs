using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.BitBucketLocal.Models;
using Repository = NuKeeper.Abstractions.CollaborationModels.Repository;

namespace NuKeeper.BitBucketLocal
{
    public class BitBucketLocalPlatform : ICollaborationPlatform
    {
        private readonly INuKeeperLogger _logger;
        private AuthSettings _settings;
        private BitbucketLocalRestClient _client;

        public BitBucketLocalPlatform(INuKeeperLogger nuKeeperLogger)
        {
            _logger = nuKeeperLogger;
        }

        public void Initialise(AuthSettings settings)
        {
            _settings = settings;
            var httpClient = new HttpClient
            {
                BaseAddress = settings.ApiBase
            };
            _client = new BitbucketLocalRestClient(httpClient, _logger, settings.Username, settings.Token);
        }

        public Task<User> GetCurrentUser()
        {
            return Task.FromResult(new User(_settings.Username, "", ""));
        }

        public async Task OpenPullRequest(ForkData target, PullRequestRequest request, IEnumerable<string> labels)
        {
            var repositories = await _client.GetGitRepositories(target.Owner);
            var targetRepository = repositories.FirstOrDefault(x => x.Name.Equals(target.Name, StringComparison.InvariantCultureIgnoreCase));

            var pullReq = new PullRequest
            {
                Title = request.Title,
                Description = request.Body,
                FromRef = new Ref
                {
                    Id = request.Head
                },
                ToRef = new Ref
                {
                    Id = request.BaseRef
                }
            };

            await _client.CreatePullRequest(pullReq, target.Owner, targetRepository.Name);
        }

        public async Task<IReadOnlyList<Organization>> GetOrganizations()
        {
            var projects = await _client.GetProjects();
            return projects
                .Select(project => new Organization(project.Name))
                .ToList();
        }

        public async Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string projectName)
        {
            var repos = await _client.GetGitRepositories(projectName);

            return repos.Select(repo =>
                    new Repository(repo.Name, false,
                        new UserPermissions(true, true, true),
                        new Uri(repo.Links.Clone.First(x => x.Name.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)).Href),
                        null, false, null))
                .ToList();
        }

        public async Task<Repository> GetUserRepository(string projectName, string repositoryName)
        {
            var repos = await GetRepositoriesForOrganisation(projectName);
            return repos.Single(x => x.Name.Equals(repositoryName, StringComparison.OrdinalIgnoreCase));
        }

        public Task<Repository> MakeUserFork(string owner, string repositoryName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RepositoryBranchExists(string projectName, string repositoryName, string branchName)
        {
            var branches = await _client.GetGitRepositoryBranches(projectName, repositoryName);

            var count = branches.Count(x => x.DisplayId.Equals(branchName, StringComparison.OrdinalIgnoreCase));
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
            var totalCount = 0;
            var repositoryFileNames = new List<string>();
            foreach (var repo in searchRequest.Repos)
            {
                repositoryFileNames.AddRange(await _client.GetGitRepositoryFileNames(repo.owner, repo.name)); 
            }

            var searchStrings = searchRequest.Term.Replace("\"", string.Empty).Split(new string[] { "OR" }, StringSplitOptions.None);

            foreach (var searchString in searchStrings)
            {
                totalCount += repositoryFileNames.FindAll(x => x.EndsWith(searchString.Trim(), StringComparison.InvariantCultureIgnoreCase)).Count;
            }
       
            return new SearchCodeResult(totalCount);
        }
    }
}
