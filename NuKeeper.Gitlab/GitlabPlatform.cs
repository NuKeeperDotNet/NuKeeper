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
    public class GitlabPlatform: ICollaborationPlatform
    {
        private readonly INuKeeperLogger _logger;
        private GitlabRestClient _client;

        public GitlabPlatform(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public void Initialise(AuthSettings settings)
        {
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

        public Task OpenPullRequest(ForkData target, PullRequestRequest request, IEnumerable<string> labels)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Organization>> GetOrganizations()
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string organisationName)
        {
            throw new NotImplementedException();
        }

        public Task<Repository> GetUserRepository(string userName, string repositoryName)
        {
            throw new NotImplementedException();
        }

        public Task<Repository> MakeUserFork(string owner, string repositoryName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RepositoryBranchExists(string userName, string repositoryName, string branchName)
        {
            throw new NotImplementedException();
        }

        public Task<SearchCodeResult> Search(SearchCodeRequest search)
        {
            throw new NotImplementedException();
        }
    }
}
