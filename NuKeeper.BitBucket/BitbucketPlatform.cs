using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.BitBucket.Models;
using Repository = NuKeeper.Abstractions.CollaborationModels.Repository;
using User = NuKeeper.Abstractions.CollaborationModels.User;

namespace NuKeeper.BitBucket
{
    public class BitbucketPlatform : ICollaborationPlatform
    {
        private readonly INuKeeperLogger _logger;
        private readonly IHttpClientFactory _clientFactory;
        private BitbucketRestClient _client;
        private AuthSettings _settings;

        public BitbucketPlatform(INuKeeperLogger logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        public void Initialise(AuthSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _client = new BitbucketRestClient(_clientFactory, _logger, settings.Username, settings.Token, settings.ApiBase);
        }

        public Task<User> GetCurrentUser()
        {
            return Task.FromResult(new User(_settings.Username, _settings.Username, _settings.Username));
        }

        public async Task<bool> PullRequestExists(ForkData target, string headBranch, string baseBranch)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var result = await _client.GetPullRequests(target.Owner, target.Name, headBranch, baseBranch);

            return result.values.Any();
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

            var repo = await _client.GetGitRepository(target.Owner, target.Name);
            var req = new PullRequest
            {
                title = request.Title,
                source = new Source
                {
                    branch = new Branch
                    {
                        name = request.Head
                    }
                },
                destination = new Source
                {
                    branch = new Branch
                    {
                        name = request.BaseRef
                    }
                },
                description = request.Body,
                close_source_branch = request.DeleteBranchAfterMerge
            };

            await _client.CreatePullRequest(target.Owner, repo.name, req);
        }

        public async Task<IReadOnlyList<Organization>> GetOrganizations()
        {
            var projects = await _client.GetProjects(_settings.Username);
            return projects
                .Select(project => new Organization(project.name))
                .ToList();
        }

        public async Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string projectName)
        {
            var repos = await _client.GetGitRepositories(projectName);
            return repos.Select(MapRepository)
                .ToList();
        }

        public async Task<Repository> GetUserRepository(string projectName, string repositoryName)
        {
            var repo = await _client.GetGitRepository(projectName, repositoryName);
            if (repo == null) return default;
            return MapRepository(repo);
        }

        public Task<Repository> MakeUserFork(string owner, string repositoryName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RepositoryBranchExists(string projectName, string repositoryName, string branchName)
        {
            var repo = await _client.GetGitRepository(projectName, repositoryName);
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

        private static Repository MapRepository(BitBucket.Models.Repository repo)
        {
            return new Repository(repo.name, false,
                    new UserPermissions(true, true, true),
                    new Uri(repo.links.html.href),
                    null, false, null);
        }

        public Task<int> GetNumberOfOpenPullRequests(string projectName, string repositoryName)
        {
            return Task.FromResult(0);
        }
    }
}
