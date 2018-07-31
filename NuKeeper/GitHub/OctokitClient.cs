using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.Logging;
using Octokit;

namespace NuKeeper.GitHub
{
    public class OctokitClient : IGitHub
    {
        private readonly INuKeeperLogger _logger;
        private readonly IGitHubClient _client;
        private readonly Uri _apiBase;

        public OctokitClient(GithubAuthSettings settings, INuKeeperLogger logger)
        {
            _logger = logger;
            _apiBase = settings.ApiBase;

            _client = new GitHubClient(new ProductHeaderValue("NuKeeper"), _apiBase)
            {
                Credentials = new Credentials(settings.Token)
            };
        }

        public async Task<Account> GetCurrentUser()
        {
            var user = await _client.User.Current();
            var userLogin = user?.Login;
            _logger.Detailed($"Read github user '{userLogin}'");
            return user;
        }

        public async Task<IReadOnlyList<Organization>> GetOrganizations()
        {
            var orgs = await _client.Organization.GetAll();
            _logger.Normal($"Read {orgs.Count} organisations");
            return orgs;
        }

        public async Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string organisationName)
        {
            var repos = await _client.Repository.GetAllForOrg(organisationName);
            _logger.Normal($"Read {repos.Count} repos for org '{organisationName}'");
            return repos;
        }

        public async Task<Repository> GetUserRepository(string userName, string repositoryName)
        {
            _logger.Detailed($"Looking for user fork for {userName}/{repositoryName}");
            try
            {
                var result = await _client.Repository.Get(userName, repositoryName);
                _logger.Normal($"User fork found at {result.GitUrl} for {result.Owner.Login}");
                return result;
            }
            catch (NotFoundException)
            {
                _logger.Detailed("User fork not found");
                return null;
            }
        }

        public async Task<Repository> MakeUserFork(string owner, string repositoryName)
        {
            _logger.Detailed($"Making user fork for {repositoryName}");
            try
            {
                var result = await _client.Repository.Forks.Create(owner, repositoryName, new NewRepositoryFork());
                _logger.Normal($"User fork created at {result.GitUrl} for {result.Owner.Login}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error("User fork not created", ex);
                return null;
            }
        }

        public async Task<Branch> GetRepositoryBranch(string userName, string repositoryName, string branchName)
        {
            try
            {
                var result = await _client.Repository.Branch.Get(userName, repositoryName, branchName);
                _logger.Detailed($"Branch found for {userName} / {repositoryName} / {branchName}");
                return result;
            }
            catch (NotFoundException)
            {
                _logger.Detailed($"No branch found for {userName} / {repositoryName} / {branchName}");
                return null;
            }
        }

        public async Task<PullRequest> OpenPullRequest(ForkData target, NewPullRequest request, IEnumerable<string> labels)
        {
            _logger.Normal($"Making PR onto '{_apiBase} {target.Owner}/{target.Name} from {request.Head}");
            _logger.Detailed($"PR title: {request.Title}");
            var createdPullRequest = await _client.PullRequest.Create(target.Owner, target.Name, request);

            await AddLabelsToIssue(target, createdPullRequest.Number, labels);

            return createdPullRequest;
        }

        public async Task<SearchCodeResult> Search(SearchCodeRequest search)
        {
            return await _client.Search.SearchCode(search);
        }

        private async Task AddLabelsToIssue(ForkData target, int issueNumber, IEnumerable<string> labels)
        {
            var labelsToApply = labels?
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();

            if (labelsToApply != null && labelsToApply.Any())
            {
                _logger.Normal(
                    $"Adding label(s) '{labelsToApply.JoinWithCommas()}' to issue "
                    + $"'{_apiBase} {target.Owner}/{target.Name} {issueNumber}'");

                try
                {
                    await _client.Issue.Labels.AddToIssue(target.Owner, target.Name, issueNumber,
                        labelsToApply);

                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to add labels. Continuing", ex);
                }
            }
        }
    }
}
