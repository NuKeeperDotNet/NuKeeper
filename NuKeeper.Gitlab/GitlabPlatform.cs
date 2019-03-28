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

        public GitlabPlatform(INuKeeperLogger logger)
        {
            _logger = logger;

        }

        public void Initialise(AuthSettings settings)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetCurrentUser()
        {
            throw new NotImplementedException();
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
