using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NuKeeper.Gitea
{
    public class GiteaPlatform : ICollaborationPlatform
    {
        private readonly INuKeeperLogger _logger;
        private readonly IHttpClientFactory _clientFactory;
        private GiteaRestClient _client;

        public GiteaPlatform(INuKeeperLogger logger, IHttpClientFactory clientFactory)
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

            _client = new GiteaRestClient(_clientFactory, settings.Token, _logger, settings.ApiBase);
        }

        public async Task<User> GetCurrentUser()
        {
            var user = await _client.GetCurrentUser();
            return new User(user.Login, user.FullName, user.Email);
        }

        public async Task<bool> PullRequestExists(ForkData target, string headBranch, string baseBranch)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var result = await _client.GetPullRequests(target.Owner, target.Name, headBranch, baseBranch);

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

            var mergeRequest = new Model.CreatePullRequestOption
            {
                Title = request.Title,
                Head = request.Head,
                Body = request.Body,
                Base = request.BaseRef,
                Labels = new long[] { 0 },

                // Gitea needs a due date for its pull requests. Maybe get this from the config file?
                DueDate = DateTime.Now.AddDays(7)
            };

            await _client.OpenPullRequest(projectName, repositoryName, mergeRequest);
        }

        public async Task<IReadOnlyList<Organization>> GetOrganizations()
        {
            var orgas = await _client.GetOrganizations();
            return orgas?.Select(x => MapOrganization(x)).ToList() ?? new List<Organization>();
        }

        public async Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string organizationName)
        {
            var repos = await _client.GetOrganizationRepositories(organizationName);
            return repos?.Select(x => MapRepository(x)).ToList() ?? new List<Repository>();
        }

        public async Task<Repository> GetUserRepository(string userName, string repositoryName)
        {
            try
            {
                var repo = await _client.GetRepository(userName, repositoryName);
                return MapRepository(repo);
            }
            catch (NuKeeperException)
            {
                _logger.Detailed("User repo not found");
                return null;
            }
        }

        public async Task<Repository> MakeUserFork(string owner, string repositoryName)
        {
            var fork = await _client.ForkRepository(owner, repositoryName, null);
            return MapRepository(fork);
        }

        /// <summary>
        /// /GET /repos/{owner}/{repo}/branches/{branch} https://try.gitea.io/api/swagger#/repository/repoGetBranch
        /// </summary>
        /// <param name="userName">the owner</param>
        /// <param name="repositoryName">the repo name</param>
        /// <param name="branchName">branch to check</param>
        /// <returns>true if exist</returns>
        public async Task<bool> RepositoryBranchExists(string userName, string repositoryName, string branchName)
        {
            var result = await _client.GetRepositoryBranch(userName, repositoryName, branchName);
            return result != null;
        }

        public Task<SearchCodeResult> Search(SearchCodeRequest search)
        {
            _logger.Error($"Search has not yet been implemented for Gitea.");
            throw new NotImplementedException();
        }

        private static Organization MapOrganization(Gitea.Model.Organization x)
        {
            return new Organization(x.FullName);
        }

        private Repository MapRepository(Gitea.Model.Repository repo)
        {
            return new Repository(
                repo.Name,
                repo.IsArchived,
                new UserPermissions(repo.Permissions.IsAdmin, repo.Permissions.IsPush, repo.Permissions.IsPull),
                new Uri(repo.CloneUrl),
                new User(repo.Owner.login, repo.Owner.full_name, repo.Owner.email),
                repo.IsFork,
                repo.Parent != null ? MapRepository(repo.Parent) : null);
        }

        public Task<int> GetNumberOfOpenPullRequests(string projectName, string repositoryName)
        {
            return Task.FromResult(0);
        }
    }
}
