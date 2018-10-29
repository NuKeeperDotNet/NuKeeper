using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.DTOs;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.Logging;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Organization = NuKeeper.Abstractions.DTOs.Organization;
using PullRequestRequest = NuKeeper.Abstractions.DTOs.PullRequestRequest;
using Repository = NuKeeper.Abstractions.DTOs.Repository;
using SearchCodeRequest = NuKeeper.Abstractions.DTOs.SearchCodeRequest;
using SearchCodeResult = NuKeeper.Abstractions.DTOs.SearchCodeResult;


namespace NuKeeper.GitHub
{
    public class OctokitClient : IGitHub
    {
        private readonly INuKeeperLogger _logger;
        private bool _initialised = false;

        private IGitHubClient _client;
        private Uri _apiBase;

        public OctokitClient(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public void Initialise(AuthSettings settings)
        {
            _apiBase = settings.ApiBase;

            _client = new GitHubClient(new ProductHeaderValue("NuKeeper"), _apiBase)
            {
                Credentials = new Credentials(settings.Token)
            };

            _initialised = true;
        }

        private void CheckInitialised()
        {
            if (!_initialised)
            {
                throw new NuKeeperException("Github has not been initialised");
            }
        }

        public async Task<Abstractions.DTOs.User> GetCurrentUser()
        {
            CheckInitialised();

            var user = await _client.User.Current();
            var userLogin = user?.Login;
            _logger.Detailed($"Read github user '{userLogin}'");
            return new GitHubUser(user);
        }

        public async Task<IReadOnlyList<Organization>> GetOrganizations()
        {
            CheckInitialised();

            var orgs = await _client.Organization.GetAll();
            _logger.Normal($"Read {orgs.Count} organisations");
            return orgs.Select(x => new GitHubOrganization(x)).ToArray();
        }

        public async Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string organisationName)
        {
            CheckInitialised();

            var repos = await _client.Repository.GetAllForOrg(organisationName);
            _logger.Normal($"Read {repos.Count} repos for org '{organisationName}'");
            return repos.Select(x => new GitHubRepository(x)).ToList();
        }

        public async Task<Repository> GetUserRepository(string userName, string repositoryName)
        {
            CheckInitialised();

            _logger.Detailed($"Looking for user fork for {userName}/{repositoryName}");
            try
            {
                var result = await _client.Repository.Get(userName, repositoryName);
                _logger.Normal($"User fork found at {result.GitUrl} for {result.Owner.Login}");
                return new GitHubRepository(result);
            }
            catch (NotFoundException)
            {
                _logger.Detailed("User fork not found");
                return null;
            }
        }

        public async Task<Repository> MakeUserFork(string owner, string repositoryName)
        {
            CheckInitialised();

            _logger.Detailed($"Making user fork for {repositoryName}");
            try
            {
                var result = await _client.Repository.Forks.Create(owner, repositoryName, new NewRepositoryFork());
                _logger.Normal($"User fork created at {result.GitUrl} for {result.Owner.Login}");
                return new GitHubRepository(result);
            }
            catch (Exception ex)
            {
                _logger.Error("User fork not created", ex);
                return null;
            }
        }

        public async Task<bool> RepositoryBranchExists(string userName, string repositoryName, string branchName)
        {
            CheckInitialised();

            try
            {
                await _client.Repository.Branch.Get(userName, repositoryName, branchName);
                _logger.Detailed($"Branch found for {userName} / {repositoryName} / {branchName}");
                return true;
            }
            catch (NotFoundException)
            {
                _logger.Detailed($"No branch found for {userName} / {repositoryName} / {branchName}");
                return false;
            }
        }

        public async Task OpenPullRequest(ForkData target, PullRequestRequest request, IEnumerable<string> labels)
        {
            CheckInitialised();

            _logger.Normal($"Making PR onto '{_apiBase} {target.Owner}/{target.Name} from {request.Head}");
            _logger.Detailed($"PR title: {request.Title}");
            var createdPullRequest = await _client.PullRequest.Create(target.Owner, target.Name, new NewPullRequest(request.Title, request.Head, request.BaseRef));

            await AddLabelsToIssue(target, createdPullRequest.Number, labels);
        }

        public async Task<SearchCodeResult> Search(SearchCodeRequest search)
        {
            CheckInitialised();
            var repos = new RepositoryCollection();
            foreach (var repo in search.Repos)
            {
                repos.Add(repo.owner, repo.name);
            }

            var result = await _client.Search.SearchCode(
                new Octokit.SearchCodeRequest(search.Term)
                {
                    Repos = repos,
                    In = new[] { CodeInQualifier.Path },
                    PerPage = search.PerPage
                });
            return new GitHubSearchCodeResult(result);
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
