using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.DTOs;
using NuKeeper.Abstractions.Logging;
using NuKeeper.BitBucket.Models;

namespace NuKeeper.BitBucket
{
    public class BitbucketPlatform : ICollaborationPlatform
    {
        private readonly INuKeeperLogger _logger;
        private BitbucketRestClient _client;
        private AuthSettings _settings;
        
        public BitbucketPlatform(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public void Initialise(AuthSettings settings)
        {
            _settings = settings;
            var httpClient = new HttpClient() { BaseAddress = settings.ApiBase};
            _client = new BitbucketRestClient(httpClient, _logger, settings.Username, settings.Token);
        }

        public Task<NuKeeper.Abstractions.DTOs.User> GetCurrentUser()
        {
            
            return Task.FromResult(new NuKeeper.Abstractions.DTOs.User(_settings.Username, "", ""));
        }

        public async Task OpenPullRequest(ForkData target, PullRequestRequest request, IEnumerable<string> labels)
        {
            var repos = await _client.GetGitRepositories(target.Owner);
            var repo = repos.Single(x => x.name == target.Name);
            var req = new PullRequest
            {
                title = request.Title,
                source = new Source()
                {
                    branch = new Branch()
                    {
                        name = request.Head
                    }
                },
                destination = new Source()
                {
                    branch = new Branch()
                    {
                        name = request.BaseRef
                    }
                },
                description = request.Body
            };

            await _client.CreatePullRequest(req, target.Owner, repo.name);
        }

        public async Task<IReadOnlyList<Organization>> GetOrganizations()
        {
            var projects = await _client.GetProjects(_settings.Username);
            return projects.Select(project => new Organization("", "")).ToList();
        }

        public async Task<IReadOnlyList<NuKeeper.Abstractions.DTOs.Repository>> GetRepositoriesForOrganisation(string projectName)
        {
            var repos = await _client.GetGitRepositories(projectName);
            return repos.Select(repo =>
                new NuKeeper.Abstractions.DTOs.Repository(repo.name, false, new UserPermissions(true, true, true), new Uri(repo.links.clone.First().href), new Uri(repo.links.clone.First().href), null, false, null))
                .ToList();
        }

        public async Task<NuKeeper.Abstractions.DTOs.Repository> GetUserRepository(string projectName, string repositoryName)
        {
            var repos = await GetRepositoriesForOrganisation(projectName);
            return repos.Single(x => x.Name.Equals(repositoryName, StringComparison.OrdinalIgnoreCase));
        }

        public Task<NuKeeper.Abstractions.DTOs.Repository> MakeUserFork(string owner, string repositoryName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RepositoryBranchExists(string projectName, string repositoryName, string branchName)
        {
            var repos = await _client.GetGitRepositories(projectName);
            var repo = repos.Single(x => x.name.Equals(repositoryName, StringComparison.OrdinalIgnoreCase));
            var refs = await _client.GetRepositoryRefs(projectName, repo.name);
            var count = refs.Count(x => x.Name.Equals(branchName, StringComparison.OrdinalIgnoreCase));
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
